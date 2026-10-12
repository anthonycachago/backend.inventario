using Inventario.Web.Domain;
using Inventario.Web.Entity;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Web.Services;

public class LineaVentaModel
{
    public int ProductoId { get; set; }
    public decimal Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
}

public class PagoModel
{
    public FormaPago FormaPago { get; set; } = FormaPago.Efectivo;
    public decimal Monto { get; set; }
    public string? Referencia { get; set; }
    public string? Banco { get; set; }
    public MarcaTarjeta? TarjetaMarca { get; set; }
    public string? TarjetaUltimos4 { get; set; }
    public string? NumeroLote { get; set; }
    public string? NumeroAutorizacion { get; set; }
    public int? Cuotas { get; set; }
    public string? Observacion { get; set; }

    public bool EsTarjeta => FormaPago is FormaPago.TarjetaCredito or FormaPago.TarjetaDebito;
    public bool RequiereReferencia => FormaPago is FormaPago.Transferencia or FormaPago.Deposito or FormaPago.Cheque;
}

public class VentaModel
{
    public int ClienteId { get; set; }
    public int? BodegaId { get; set; }
    public CondicionPago CondicionPago { get; set; } = CondicionPago.Contado;
    public List<LineaVentaModel> Lineas { get; set; } = [];
    /// <summary>Contado: deben sumar el total. Crédito: abono inicial opcional.</summary>
    public List<PagoModel> Pagos { get; set; } = [];
    public string? Observacion { get; set; }
}

public record TotalesVenta(decimal Subtotal0, decimal SubtotalIva, decimal Iva, decimal Total);

public static class ReglasVentas
{
    /// <summary>Monto máximo de una factura a consumidor final según el SRI.</summary>
    public const decimal MontoMaximoConsumidorFinal = 50m;

    public static decimal SubtotalLinea(decimal cantidad, decimal precio) =>
        Math.Round(cantidad * precio, 2, MidpointRounding.AwayFromZero);

    /// <summary>IVA calculado por tarifa sobre la suma de subtotales (como en el comprobante electrónico).</summary>
    public static TotalesVenta Calcular(IEnumerable<(decimal Cantidad, decimal Precio, decimal Tarifa)> lineas)
    {
        var porTarifa = lineas
            .GroupBy(l => l.Tarifa)
            .Select(g => (Tarifa: g.Key, Base: g.Sum(l => SubtotalLinea(l.Cantidad, l.Precio))))
            .ToList();

        var subtotal0 = porTarifa.Where(t => t.Tarifa == 0).Sum(t => t.Base);
        var subtotalIva = porTarifa.Where(t => t.Tarifa > 0).Sum(t => t.Base);
        var iva = porTarifa.Where(t => t.Tarifa > 0).Sum(t => Math.Round(t.Base * t.Tarifa / 100m, 2, MidpointRounding.AwayFromZero));
        return new TotalesVenta(subtotal0, subtotalIva, iva, subtotal0 + subtotalIva + iva);
    }

    /// <summary>Valida un pago y devuelve los datos normalizados. Nunca acepta más de 4 dígitos de tarjeta.</summary>
    public static Pago CrearPago(PagoModel m)
    {
        if (m.Monto <= 0) throw new ReglaNegocioException("El monto del pago debe ser mayor a 0.");
        if (decimal.Round(m.Monto, 2) != m.Monto) throw new ReglaNegocioException("El monto del pago admite máximo 2 decimales.");

        var pago = new Pago
        {
            FormaPago = m.FormaPago,
            Monto = m.Monto,
            Fecha = DateTime.Now,
            Observacion = Limpiar(m.Observacion),
            UsuarioId = UsuarioSistema.Id
        };

        if (m.EsTarjeta)
        {
            var ultimos4 = m.TarjetaUltimos4?.Trim() ?? string.Empty;
            if (ultimos4.Length != 4 || !ultimos4.All(char.IsAsciiDigit))
                throw new ReglaNegocioException("Ingrese solo los ÚLTIMOS 4 dígitos de la tarjeta (nunca el número completo).");
            if (m.TarjetaMarca is null) throw new ReglaNegocioException("Seleccione la marca de la tarjeta.");
            if (string.IsNullOrWhiteSpace(m.NumeroAutorizacion))
                throw new ReglaNegocioException("Ingrese el número de autorización del voucher.");
            if (m.FormaPago == FormaPago.TarjetaCredito && m.Cuotas is < 1 or > 48)
                throw new ReglaNegocioException("Las cuotas deben estar entre 1 y 48.");

            pago.TarjetaMarca = m.TarjetaMarca;
            pago.TarjetaUltimos4 = ultimos4;
            pago.NumeroAutorizacion = m.NumeroAutorizacion.Trim();
            pago.NumeroLote = Limpiar(m.NumeroLote);
            pago.Cuotas = m.FormaPago == FormaPago.TarjetaCredito ? m.Cuotas ?? 1 : null;
        }
        else if (m.RequiereReferencia)
        {
            if (string.IsNullOrWhiteSpace(m.Referencia))
                throw new ReglaNegocioException($"Ingrese el número de {(m.FormaPago == FormaPago.Cheque ? "cheque" : "comprobante")}.");
            pago.Referencia = m.Referencia.Trim();
            pago.Banco = Limpiar(m.Banco);
        }

        return pago;
    }

    private static string? Limpiar(string? valor) => string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
}

/// <summary>
/// Venta = despacho FEFO + cuenta por cobrar en una sola transacción. Los cobros (contado, abonos
/// y tarjetas registradas desde el voucher del datáfono) actualizan el saldo con concurrencia optimista.
/// </summary>
public class VentasService(IDbContextFactory<AppDbContext> dbFactory)
{
    public async Task<Venta> RegistrarVentaAsync(VentaModel m)
    {
        if (m.Lineas.Count == 0) throw new ReglaNegocioException("Agregue al menos un producto.");
        if (m.Lineas.Any(l => l.Cantidad <= 0 || l.PrecioUnitario < 0))
            throw new ReglaNegocioException("Las cantidades deben ser mayores a 0 y los precios no pueden ser negativos.");

        await using var db = await dbFactory.CreateDbContextAsync();

        var cliente = await db.Clientes.FirstOrDefaultAsync(c => c.Id == m.ClienteId && c.Activo)
            ?? throw new ReglaNegocioException("Seleccione un cliente activo.");

        var ids = m.Lineas.Select(l => l.ProductoId).Distinct().ToList();
        var productos = await db.Productos.Where(p => ids.Contains(p.Id) && p.Activo).ToDictionaryAsync(p => p.Id);
        if (productos.Count != ids.Count) throw new ReglaNegocioException("Hay productos inexistentes o inactivos en la venta.");

        var totales = ReglasVentas.Calcular(m.Lineas.Select(l => (l.Cantidad, l.PrecioUnitario, productos[l.ProductoId].TarifaIva)));
        if (totales.Total <= 0) throw new ReglaNegocioException("El total de la venta debe ser mayor a 0.");

        if (cliente.TipoIdentificacion == TipoIdentificacion.ConsumidorFinal && totales.Total > ReglasVentas.MontoMaximoConsumidorFinal)
            throw new ReglaNegocioException(
                $"Las ventas a consumidor final no pueden superar {ReglasVentas.MontoMaximoConsumidorFinal:C2}: registre los datos del cliente.");

        var pagos = m.Pagos.Select(ReglasVentas.CrearPago).ToList();
        var pagado = pagos.Sum(p => p.Monto);

        DateTime? vencimiento = null;
        if (m.CondicionPago == CondicionPago.Contado)
        {
            if (pagado != totales.Total)
                throw new ReglaNegocioException($"Una venta de contado debe pagarse completa: total {totales.Total:C2}, pagos {pagado:C2}.");
        }
        else
        {
            if (cliente.LimiteCredito <= 0 || cliente.DiasCredito <= 0)
                throw new ReglaNegocioException($"{cliente.RazonSocial} no tiene crédito aprobado (límite y días de crédito).");
            if (pagado >= totales.Total)
                throw new ReglaNegocioException("El abono inicial cubre todo el total: registre la venta como contado.");

            var hoy = DateTime.Today;
            var cartera = await db.Ventas
                .Where(v => v.ClienteId == cliente.Id && v.Estado == EstadoVenta.Pendiente)
                .Select(v => new { v.Saldo, v.FechaVencimiento })
                .ToListAsync();
            if (cartera.Any(v => v.FechaVencimiento < hoy))
                throw new ReglaNegocioException($"{cliente.RazonSocial} tiene facturas vencidas: debe ponerse al día antes de comprar a crédito.");

            var nuevoSaldo = cartera.Sum(v => v.Saldo) + totales.Total - pagado;
            if (nuevoSaldo > cliente.LimiteCredito)
                throw new ReglaNegocioException(
                    $"Excede el límite de crédito: saldo resultante {nuevoSaldo:C2}, límite {cliente.LimiteCredito:C2}.");
            vencimiento = hoy.AddDays(cliente.DiasCredito);
        }

        // ToListAsync y no SingleAsync: componer la consulta la envuelve en una subconsulta,
        // y SQL Server no permite NEXT VALUE FOR dentro de subconsultas.
        var secuencia = (await db.Database
            .SqlQueryRaw<long>($"SELECT NEXT VALUE FOR [{AppDbContext.Schema}].[{AppDbContext.SecuenciaVentas}] AS [Value]")
            .ToListAsync()).Single();

        var venta = new Venta
        {
            Numero = $"V-{secuencia:000000}",
            Fecha = DateTime.Now,
            ClienteId = cliente.Id,
            BodegaId = m.BodegaId,
            CondicionPago = m.CondicionPago,
            FechaVencimiento = vencimiento,
            Subtotal0 = totales.Subtotal0,
            SubtotalIva = totales.SubtotalIva,
            Iva = totales.Iva,
            Total = totales.Total,
            Saldo = totales.Total - pagado,
            Estado = totales.Total - pagado == 0 ? EstadoVenta.Pagada : EstadoVenta.Pendiente,
            Observacion = string.IsNullOrWhiteSpace(m.Observacion) ? null : m.Observacion.Trim(),
            UsuarioId = UsuarioSistema.Id
        };
        foreach (var l in m.Lineas)
        {
            venta.Detalles.Add(new VentaDetalle
            {
                ProductoId = l.ProductoId,
                Cantidad = l.Cantidad,
                PrecioUnitario = l.PrecioUnitario,
                TarifaIva = productos[l.ProductoId].TarifaIva,
                Subtotal = ReglasVentas.SubtotalLinea(l.Cantidad, l.PrecioUnitario)
            });
        }
        foreach (var p in pagos) venta.Pagos.Add(p);
        db.Ventas.Add(venta);

        // Despacho FEFO dentro de la misma transacción: si falta stock no se guarda nada
        foreach (var l in m.Lineas)
            await InventarioService.ConsumirFefoAsync(db, l.ProductoId, m.BodegaId, l.Cantidad, venta.Numero, $"Venta a {cliente.RazonSocial}", venta);

        await InventarioService.GuardarAsync(db);
        return venta;
    }

    /// <summary>Abono o pago de una venta pendiente.</summary>
    public async Task<Pago> RegistrarPagoAsync(int ventaId, PagoModel m)
    {
        var pago = ReglasVentas.CrearPago(m);

        await using var db = await dbFactory.CreateDbContextAsync();
        var venta = await db.Ventas.FirstOrDefaultAsync(v => v.Id == ventaId)
            ?? throw new ReglaNegocioException("La venta no existe.");
        if (venta.Saldo == 0) throw new ReglaNegocioException($"La venta {venta.Numero} ya está pagada.");
        if (pago.Monto > venta.Saldo)
            throw new ReglaNegocioException($"El pago ({pago.Monto:C2}) supera el saldo pendiente ({venta.Saldo:C2}).");

        venta.Pagos.Add(pago);
        venta.Saldo -= pago.Monto;
        venta.Estado = venta.Saldo == 0 ? EstadoVenta.Pagada : EstadoVenta.Pendiente;

        await InventarioService.GuardarAsync(db);
        return pago;
    }

    /// <summary>Anula un pago mal registrado (el saldo vuelve a la cuenta por cobrar). No se borra: queda el rastro.</summary>
    public async Task AnularPagoAsync(int pagoId, string motivo)
    {
        if (string.IsNullOrWhiteSpace(motivo)) throw new ReglaNegocioException("Indique el motivo de la anulación.");

        await using var db = await dbFactory.CreateDbContextAsync();
        var pago = await db.Pagos.Include(p => p.Venta).FirstOrDefaultAsync(p => p.Id == pagoId)
            ?? throw new ReglaNegocioException("El pago no existe.");
        if (pago.Anulado) throw new ReglaNegocioException("El pago ya estaba anulado.");

        pago.Anulado = true;
        pago.MotivoAnulacion = motivo.Trim();
        pago.Venta.Saldo += pago.Monto;
        pago.Venta.Estado = EstadoVenta.Pendiente;

        await InventarioService.GuardarAsync(db);
    }
}
