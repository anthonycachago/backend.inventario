namespace Inventario.Web.Entity;

// Entidades sin clave mapeadas a vistas SQL (ver migración "LocalizacionVistasYReglas").
// Solo lectura: se usan en el dashboard y los reportes.

/// <summary>inv.vw_StockPorProducto: stock total por producto contra su stock mínimo.</summary>
public class StockProductoVista
{
    public int ProductoId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Producto { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public string UnidadMedida { get; set; } = string.Empty;
    public decimal StockMinimo { get; set; }
    public decimal StockTotal { get; set; }
    public bool BajoMinimo { get; set; }
}

/// <summary>inv.vw_StockPorUbicacion: detalle de existencias con bodega, ubicación, lote y vencimiento.</summary>
public class StockUbicacionVista
{
    public int ExistenciaId { get; set; }
    public int BodegaId { get; set; }
    public string Bodega { get; set; } = string.Empty;
    public int UbicacionId { get; set; }
    public string Ubicacion { get; set; } = string.Empty;
    public string Zona { get; set; } = string.Empty;
    public string TipoUbicacion { get; set; } = string.Empty;
    public int LoteId { get; set; }
    public string Lote { get; set; } = string.Empty;
    public int ProductoId { get; set; }
    public string Producto { get; set; } = string.Empty;
    public string UnidadMedida { get; set; } = string.Empty;
    public decimal Cantidad { get; set; }
    public decimal CostoUnitario { get; set; }
    public DateTime FechaVencimiento { get; set; }
    public int DiasParaVencer { get; set; }
}

/// <summary>inv.vw_OcupacionUbicaciones: ocupación en kg y última lectura de temperatura por ubicación.</summary>
public class OcupacionUbicacionVista
{
    public int UbicacionId { get; set; }
    public int BodegaId { get; set; }
    public string Bodega { get; set; } = string.Empty;
    public string Codigo { get; set; } = string.Empty;
    public string Zona { get; set; } = string.Empty;
    public string? Pasillo { get; set; }
    public string? Rack { get; set; }
    public string? Nivel { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public bool Activo { get; set; }
    public decimal? CapacidadKg { get; set; }
    public decimal OcupadoKg { get; set; }
    public decimal? PorcentajeOcupacion { get; set; }
    public int LotesDistintos { get; set; }
    public decimal? TemperaturaObjetivo { get; set; }
    public decimal? UltimaTemperatura { get; set; }
    public DateTime? FechaUltimaTemperatura { get; set; }
}
