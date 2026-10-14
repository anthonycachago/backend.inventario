namespace Inventario.Web.Entity;

public class Cliente
{
    public int Id { get; set; }
    public TipoIdentificacion TipoIdentificacion { get; set; } = TipoIdentificacion.Ruc;
    public string Identificacion { get; set; } = string.Empty;
    public string RazonSocial { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public string? Direccion { get; set; }
    /// <summary>Saldo máximo pendiente permitido en ventas a crédito (0 = solo contado).</summary>
    public decimal LimiteCredito { get; set; }
    public int DiasCredito { get; set; }
    public bool Activo { get; set; } = true;

    public ICollection<Venta> Ventas { get; set; } = new List<Venta>();
}

public class Venta
{
    public int Id { get; set; }
    /// <summary>V-000001 (secuencia inv.VentaNumero).</summary>
    public string Numero { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public CondicionPago CondicionPago { get; set; }
    /// <summary>Solo en crédito: Fecha + días de crédito del cliente.</summary>
    public DateTime? FechaVencimiento { get; set; }
    public decimal Subtotal0 { get; set; }
    public decimal SubtotalIva { get; set; }
    public decimal Iva { get; set; }
    public decimal Total { get; set; }
    /// <summary>Total menos pagos vigentes. Se actualiza en la misma transacción que cada pago.</summary>
    public decimal Saldo { get; set; }
    public EstadoVenta Estado { get; set; }
    public string? Observacion { get; set; }
    public byte[] RowVersion { get; set; } = [];

    public int ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;

    public int? BodegaId { get; set; }
    public Bodega? Bodega { get; set; }

    public string UsuarioId { get; set; } = string.Empty;
    public ApplicationUser Usuario { get; set; } = null!;

    public ICollection<VentaDetalle> Detalles { get; set; } = new List<VentaDetalle>();
    public ICollection<Pago> Pagos { get; set; } = new List<Pago>();
    public ICollection<MovimientoInventario> Movimientos { get; set; } = new List<MovimientoInventario>();
}

public class VentaDetalle
{
    public int Id { get; set; }
    public decimal Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    /// <summary>Porcentaje de IVA aplicado (copiado del producto al momento de la venta).</summary>
    public decimal TarifaIva { get; set; }
    public decimal Subtotal { get; set; }

    public int VentaId { get; set; }
    public Venta Venta { get; set; } = null!;

    public int ProductoId { get; set; }
    public Producto Producto { get; set; } = null!;
}

/// <summary>
/// Cobro de una venta. De tarjetas solo se guardan datos del voucher (marca, últimos 4 dígitos,
/// lote, autorización): nunca el número completo, la fecha de expiración ni el CVV.
/// </summary>
public class Pago
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; }
    public decimal Monto { get; set; }
    public FormaPago FormaPago { get; set; }
    /// <summary>Nº de transferencia, depósito o cheque.</summary>
    public string? Referencia { get; set; }
    public string? Banco { get; set; }
    public MarcaTarjeta? TarjetaMarca { get; set; }
    public string? TarjetaUltimos4 { get; set; }
    public string? NumeroLote { get; set; }
    public string? NumeroAutorizacion { get; set; }
    /// <summary>Meses de diferido (1 = corriente).</summary>
    public int? Cuotas { get; set; }
    public string? Observacion { get; set; }
    public bool Anulado { get; set; }
    public string? MotivoAnulacion { get; set; }

    public int VentaId { get; set; }
    public Venta Venta { get; set; } = null!;

    public string UsuarioId { get; set; } = string.Empty;
    public ApplicationUser Usuario { get; set; } = null!;
}
