using System.ComponentModel.DataAnnotations;
using Inventario.Web.Domain;
using Inventario.Web.Entity;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Web.Services;

public class EntradaModel
{
    [Range(1, int.MaxValue, ErrorMessage = "Seleccione un producto.")]
    public int ProductoId { get; set; }
    public int? ProveedorId { get; set; }
    [Range(1, int.MaxValue, ErrorMessage = "Seleccione una ubicación.")]
    public int UbicacionId { get; set; }
    [Required(ErrorMessage = "El código de lote es obligatorio."), StringLength(40)]
    public string CodigoLote { get; set; } = string.Empty;
    [Range(0.001, 999_999_999, ErrorMessage = "La cantidad debe ser mayor a 0.")]
    public decimal Cantidad { get; set; }
    [Range(0, 999_999_999, ErrorMessage = "El costo no puede ser negativo.")]
    public decimal CostoUnitario { get; set; }
    public DateTime? FechaCaptura { get; set; }
    /// <summary>Si se deja vacío se calcula con los días de vida útil del producto.</summary>
    public DateTime? FechaVencimiento { get; set; }
    [StringLength(200)] public string? Origen { get; set; }
    [StringLength(100)] public string? CertificadoSanitario { get; set; }
    [StringLength(50)] public string? Referencia { get; set; }
    [StringLength(250)] public string? Observacion { get; set; }
}

public class MovimientoModel
{
    public TipoMovimiento Tipo { get; set; } = TipoMovimiento.Salida;
    public int LoteId { get; set; }
    public int UbicacionOrigenId { get; set; }
    public int UbicacionDestinoId { get; set; }
    /// <summary>En ajustes es la cantidad contada físicamente; en los demás, la cantidad a mover.</summary>
    [Range(0, 999_999_999, ErrorMessage = "La cantidad no puede ser negativa.")]
    public decimal Cantidad { get; set; }
    [StringLength(50)] public string? Referencia { get; set; }
    [StringLength(250)] public string? Observacion { get; set; }
}

public class SalidaFefoModel
{
    [Range(1, int.MaxValue, ErrorMessage = "Seleccione un producto.")]
    public int ProductoId { get; set; }
    public int? BodegaId { get; set; }
    [Range(0.001, 999_999_999, ErrorMessage = "La cantidad debe ser mayor a 0.")]
    public decimal Cantidad { get; set; }
    [StringLength(50)] public string? Referencia { get; set; }
    [StringLength(250)] public string? Observacion { get; set; }
}

public record ConsumoFefo(string Lote, string Ubicacion, DateTime FechaVencimiento, decimal Cantidad);

/// <summary>
/// Única puerta de entrada para modificar stock: cada operación actualiza Existencias
/// y registra el movimiento del kardex en la misma transacción (un solo SaveChanges).
/// </summary>
public class InventarioService(IDbContextFactory<AppDbContext> dbFactory)
{
    public async Task<Lote> RegistrarEntradaAsync(EntradaModel m)
    {
        if (m.Cantidad <= 0) throw new ReglaNegocioException("La cantidad debe ser mayor a 0.");

        await using var db = await dbFactory.CreateDbContextAsync();

        var producto = await db.Productos.FirstOrDefaultAsync(p => p.Id == m.ProductoId && p.Activo)
            ?? throw new ReglaNegocioException("El producto no existe o está inactivo.");
        var ubicacion = await ObtenerUbicacionActivaAsync(db, m.UbicacionId);
        ValidarCompatibilidad(producto, ubicacion);
        await ValidarCapacidadAsync(db, ubicacion, producto, m.Cantidad);

        var codigo = m.CodigoLote.Trim().ToUpperInvariant();
        var lote = await db.Lotes.FirstOrDefaultAsync(l => l.ProductoId == producto.Id && l.Codigo == codigo);
        if (lote is null)
        {
            var hoy = DateTime.Today;
            var vencimiento = (m.FechaVencimiento ?? (m.FechaCaptura ?? hoy).AddDays(producto.DiasVidaUtil)).Date;
            if (vencimiento < hoy)
                throw new ReglaNegocioException($"El lote vence el {vencimiento:dd/MM/yyyy}: no se puede recibir producto vencido.");
            if (m.FechaCaptura > hoy)
                throw new ReglaNegocioException("La fecha de captura no puede ser futura.");

            lote = new Lote
            {
                Codigo = codigo,
                ProductoId = producto.Id,
                ProveedorId = m.ProveedorId,
                FechaIngreso = DateTime.Now,
                FechaCaptura = m.FechaCaptura?.Date,
                FechaVencimiento = vencimiento,
                Origen = m.Origen,
                CertificadoSanitario = m.CertificadoSanitario,
                CostoUnitario = m.CostoUnitario
            };
            db.Lotes.Add(lote);
        }

        await AplicarAsync(db, lote, null, ubicacion.Id, m.Cantidad, TipoMovimiento.Entrada, m.Referencia, m.Observacion);
        await GuardarAsync(db);
        return lote;
    }

    public async Task RegistrarMovimientoAsync(MovimientoModel m)
    {
        await using var db = await dbFactory.CreateDbContextAsync();

        var lote = await db.Lotes.Include(l => l.Producto).FirstOrDefaultAsync(l => l.Id == m.LoteId)
            ?? throw new ReglaNegocioException("Seleccione un lote.");
        if (m.UbicacionOrigenId == 0)
            throw new ReglaNegocioException("Seleccione la ubicación de origen.");

        switch (m.Tipo)
        {
            case TipoMovimiento.Salida:
                if (lote.FechaVencimiento.Date < DateTime.Today)
                    throw new ReglaNegocioException("El lote está vencido: regístrelo como merma, no como salida.");
                ValidarCantidadPositiva(m.Cantidad);
                await AplicarAsync(db, lote, m.UbicacionOrigenId, null, m.Cantidad, m.Tipo, m.Referencia, m.Observacion);
                break;

            case TipoMovimiento.Merma:
                ValidarCantidadPositiva(m.Cantidad);
                if (string.IsNullOrWhiteSpace(m.Observacion))
                    throw new ReglaNegocioException("Indique el motivo de la merma en la observación.");
                await AplicarAsync(db, lote, m.UbicacionOrigenId, null, m.Cantidad, m.Tipo, m.Referencia, m.Observacion);
                break;

            case TipoMovimiento.Transferencia:
                ValidarCantidadPositiva(m.Cantidad);
                if (m.UbicacionDestinoId == 0 || m.UbicacionDestinoId == m.UbicacionOrigenId)
                    throw new ReglaNegocioException("Seleccione una ubicación de destino distinta a la de origen.");
                var destino = await ObtenerUbicacionActivaAsync(db, m.UbicacionDestinoId);
                ValidarCompatibilidad(lote.Producto, destino);
                await ValidarCapacidadAsync(db, destino, lote.Producto, m.Cantidad);
                await AplicarAsync(db, lote, m.UbicacionOrigenId, destino.Id, m.Cantidad, m.Tipo, m.Referencia, m.Observacion);
                break;

            case TipoMovimiento.Ajuste:
                // Conteo físico: se registra solo la diferencia contra el sistema.
                var actual = await db.Existencias
                    .Where(e => e.LoteId == lote.Id && e.UbicacionId == m.UbicacionOrigenId)
                    .Select(e => (decimal?)e.Cantidad).FirstOrDefaultAsync() ?? 0m;
                var diferencia = m.Cantidad - actual;
                if (diferencia == 0)
                    throw new ReglaNegocioException("La cantidad contada coincide con el sistema: no hay nada que ajustar.");
                if (string.IsNullOrWhiteSpace(m.Observacion))
                    throw new ReglaNegocioException("Indique el motivo del ajuste en la observación.");
                if (diferencia > 0)
                    await AplicarAsync(db, lote, null, m.UbicacionOrigenId, diferencia, m.Tipo, m.Referencia, m.Observacion);
                else
                    await AplicarAsync(db, lote, m.UbicacionOrigenId, null, -diferencia, m.Tipo, m.Referencia, m.Observacion);
                break;

            default:
                throw new ReglaNegocioException("Las entradas se registran desde la pantalla de recepción.");
        }

        await GuardarAsync(db);
    }

    /// <summary>
    /// Despacho FEFO (primero en vencer, primero en salir): consume los lotes vigentes con vencimiento
    /// más próximo. No toma stock en cuarentena ni lotes vencidos.
    /// </summary>
    public async Task<List<ConsumoFefo>> RegistrarSalidaFefoAsync(SalidaFefoModel m)
    {
        if (m.Cantidad <= 0) throw new ReglaNegocioException("La cantidad debe ser mayor a 0.");

        await using var db = await dbFactory.CreateDbContextAsync();
        var consumos = await ConsumirFefoAsync(db, m.ProductoId, m.BodegaId, m.Cantidad, m.Referencia, m.Observacion);
        await GuardarAsync(db);
        return consumos;
    }

    /// <summary>
    /// Agrega al contexto las salidas FEFO sin guardar, para que otra operación (p. ej. una venta)
    /// las confirme en su misma transacción.
    /// </summary>
    internal static async Task<List<ConsumoFefo>> ConsumirFefoAsync(AppDbContext db, int productoId, int? bodegaId,
        decimal cantidad, string? referencia, string? observacion, Venta? venta = null)
    {
        var hoy = DateTime.Today;
        var candidatos = (await db.Existencias
                .Include(e => e.Lote)
                .Include(e => e.Ubicacion)
                .Where(e => e.Lote.ProductoId == productoId
                            && e.Cantidad > 0
                            && e.Lote.FechaVencimiento >= hoy
                            && e.Ubicacion.Activo
                            && e.Ubicacion.Tipo != TipoUbicacion.Cuarentena
                            && (bodegaId == null || e.Ubicacion.BodegaId == bodegaId))
                .ToListAsync())
            // Las ya consumidas en este mismo contexto vuelven con su valor en memoria
            .Where(e => e.Cantidad > 0 && db.Entry(e).State != EntityState.Deleted)
            .OrderBy(e => e.Lote.FechaVencimiento)
            .ThenBy(e => e.Lote.FechaIngreso)
            .ThenBy(e => e.Cantidad)
            .ToList();

        var disponible = candidatos.Sum(e => e.Cantidad);
        if (disponible < cantidad)
        {
            var nombre = await db.Productos.Where(p => p.Id == productoId).Select(p => p.Nombre).FirstOrDefaultAsync();
            throw new ReglaNegocioException($"Stock disponible insuficiente de {nombre}: hay {disponible:N3} y se solicitan {cantidad:N3}.");
        }

        var pendiente = cantidad;
        var consumos = new List<ConsumoFefo>();
        foreach (var e in candidatos)
        {
            if (pendiente == 0) break;
            var tomar = Math.Min(pendiente, e.Cantidad);
            await AplicarAsync(db, e.Lote, e.UbicacionId, null, tomar, TipoMovimiento.Salida, referencia, observacion, venta);
            consumos.Add(new ConsumoFefo(e.Lote.Codigo, e.Ubicacion.Codigo, e.Lote.FechaVencimiento, tomar));
            pendiente -= tomar;
        }
        return consumos;
    }

    /// <summary>Registra una lectura de temperatura. Devuelve true si está fuera del rango tolerado.</summary>
    public async Task<bool> RegistrarTemperaturaAsync(int ubicacionId, decimal temperatura, string? observacion)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var ubicacion = await ObtenerUbicacionActivaAsync(db, ubicacionId);
        var fuera = ReglasInventario.TemperaturaFueraDeRango(ubicacion.TemperaturaObjetivo, temperatura);
        if (fuera && string.IsNullOrWhiteSpace(observacion))
            throw new ReglaNegocioException("La temperatura está fuera de rango: registre la acción correctiva en la observación.");

        db.RegistrosTemperatura.Add(new RegistroTemperatura
        {
            UbicacionId = ubicacion.Id,
            Temperatura = temperatura,
            FechaHora = DateTime.Now,
            Observacion = observacion,
            UsuarioId = UsuarioSistema.Id
        });
        await db.SaveChangesAsync();
        return fuera;
    }

    // ---------------------------------------------------------------- helpers

    /// <summary>Mueve stock entre ubicaciones (null = fuera del inventario) y agrega el movimiento al kardex.</summary>
    private static async Task AplicarAsync(AppDbContext db, Lote lote, int? origenId, int? destinoId, decimal cantidad,
        TipoMovimiento tipo, string? referencia, string? observacion, Venta? venta = null)
    {
        if (origenId is not null)
        {
            var origen = await BuscarExistenciaAsync(db, lote, origenId.Value);
            if (origen is null || origen.Cantidad < cantidad)
                throw new ReglaNegocioException(
                    $"Stock insuficiente del lote {lote.Codigo} en la ubicación de origen: disponible {origen?.Cantidad ?? 0:N3}, solicitado {cantidad:N3}.");

            origen.Cantidad -= cantidad;
            origen.FechaActualizacion = DateTime.Now;
            if (origen.Cantidad == 0) db.Existencias.Remove(origen);
        }

        if (destinoId is not null)
        {
            var destino = await BuscarExistenciaAsync(db, lote, destinoId.Value);
            if (destino is null)
            {
                destino = new Existencia { Lote = lote, UbicacionId = destinoId.Value };
                db.Existencias.Add(destino);
            }
            else if (db.Entry(destino).State == EntityState.Deleted)
            {
                db.Entry(destino).State = EntityState.Modified;
            }
            destino.Cantidad += cantidad;
            destino.FechaActualizacion = DateTime.Now;
        }

        db.MovimientosInventario.Add(new MovimientoInventario
        {
            Lote = lote,
            Tipo = tipo,
            Cantidad = cantidad,
            Fecha = DateTime.Now,
            UbicacionOrigenId = origenId,
            UbicacionDestinoId = destinoId,
            Referencia = string.IsNullOrWhiteSpace(referencia) ? null : referencia.Trim(),
            Observacion = string.IsNullOrWhiteSpace(observacion) ? null : observacion.Trim(),
            UsuarioId = UsuarioSistema.Id,
            Venta = venta
        });
    }

    /// <summary>Busca primero en lo que ya está siendo rastreado (para varias operaciones en un mismo guardado).</summary>
    private static async Task<Existencia?> BuscarExistenciaAsync(AppDbContext db, Lote lote, int ubicacionId)
    {
        var local = db.ChangeTracker.Entries<Existencia>()
            .Select(e => e.Entity)
            .FirstOrDefault(e => (e.Lote == lote || (lote.Id != 0 && e.LoteId == lote.Id)) && e.UbicacionId == ubicacionId);
        if (local is not null || lote.Id == 0) return local;

        return await db.Existencias.FirstOrDefaultAsync(e => e.LoteId == lote.Id && e.UbicacionId == ubicacionId);
    }

    private static async Task<Ubicacion> ObtenerUbicacionActivaAsync(AppDbContext db, int id) =>
        await db.Ubicaciones.FirstOrDefaultAsync(u => u.Id == id && u.Activo)
        ?? throw new ReglaNegocioException("La ubicación no existe o está inactiva.");

    private static void ValidarCompatibilidad(Producto producto, Ubicacion ubicacion)
    {
        if (!ReglasInventario.EsCompatible(producto.EstadoConservacion, ubicacion.Tipo))
            throw new ReglaNegocioException(
                $"Un producto {producto.EstadoConservacion} no puede almacenarse en {ubicacion.Codigo} ({ubicacion.Tipo}).");
    }

    private static async Task ValidarCapacidadAsync(AppDbContext db, Ubicacion ubicacion, Producto producto, decimal cantidad)
    {
        var factor = ReglasInventario.FactorKg(producto.UnidadMedida);
        if (ubicacion.CapacidadKg is null || factor is null) return;

        var existencias = await db.Existencias
            .Where(e => e.UbicacionId == ubicacion.Id)
            .Select(e => new { e.Cantidad, e.Lote.Producto.UnidadMedida })
            .ToListAsync();
        var ocupado = existencias.Sum(e => e.Cantidad * (ReglasInventario.FactorKg(e.UnidadMedida) ?? 0));
        var nuevo = ocupado + cantidad * factor.Value;

        if (nuevo > ubicacion.CapacidadKg)
            throw new ReglaNegocioException(
                $"La ubicación {ubicacion.Codigo} excede su capacidad: {nuevo:N1} kg de {ubicacion.CapacidadKg:N1} kg.");
    }

    private static void ValidarCantidadPositiva(decimal cantidad)
    {
        if (cantidad <= 0) throw new ReglaNegocioException("La cantidad debe ser mayor a 0.");
    }

    internal static async Task GuardarAsync(AppDbContext db)
    {
        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ReglaNegocioException("Otro usuario modificó estos datos al mismo tiempo. Recargue e intente de nuevo.");
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("duplicate", StringComparison.OrdinalIgnoreCase) == true)
        {
            throw new ReglaNegocioException("Otro usuario registró el mismo lote o existencia al mismo tiempo. Intente de nuevo.");
        }
    }
}
