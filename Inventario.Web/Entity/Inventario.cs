namespace Inventario.Web.Entity;

/// <summary>Lote de recepción: permite trazabilidad (origen, captura, vencimiento).</summary>
public class Lote
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public DateTime FechaIngreso { get; set; }
    public DateTime? FechaCaptura { get; set; }
    public DateTime FechaVencimiento { get; set; }
    /// <summary>Zona de pesca, embarcación o camaronera de origen.</summary>
    public string? Origen { get; set; }
    public string? CertificadoSanitario { get; set; }
    public decimal CostoUnitario { get; set; }

    public int ProductoId { get; set; }
    public Producto Producto { get; set; } = null!;

    public int? ProveedorId { get; set; }
    public Proveedor? Proveedor { get; set; }

    public ICollection<Existencia> Existencias { get; set; } = new List<Existencia>();
    public ICollection<MovimientoInventario> Movimientos { get; set; } = new List<MovimientoInventario>();
}

/// <summary>Stock actual de un lote en una ubicación (una fila por lote + ubicación).</summary>
public class Existencia
{
    public int Id { get; set; }
    public decimal Cantidad { get; set; }
    public DateTime FechaActualizacion { get; set; }
    public byte[] RowVersion { get; set; } = [];

    public int LoteId { get; set; }
    public Lote Lote { get; set; } = null!;

    public int UbicacionId { get; set; }
    public Ubicacion Ubicacion { get; set; } = null!;
}

/// <summary>Kardex: historial de entradas, salidas, transferencias, ajustes y mermas.</summary>
public class MovimientoInventario
{
    public long Id { get; set; }
    public TipoMovimiento Tipo { get; set; }
    public decimal Cantidad { get; set; }
    public DateTime Fecha { get; set; }
    /// <summary>Nº de factura, guía de remisión, orden, etc.</summary>
    public string? Referencia { get; set; }
    public string? Observacion { get; set; }

    public int LoteId { get; set; }
    public Lote Lote { get; set; } = null!;

    /// <summary>Null en entradas.</summary>
    public int? UbicacionOrigenId { get; set; }
    public Ubicacion? UbicacionOrigen { get; set; }

    /// <summary>Null en salidas y mermas.</summary>
    public int? UbicacionDestinoId { get; set; }
    public Ubicacion? UbicacionDestino { get; set; }

    public string UsuarioId { get; set; } = string.Empty;
    public ApplicationUser Usuario { get; set; } = null!;
}
