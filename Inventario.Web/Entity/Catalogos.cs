namespace Inventario.Web.Entity;

/// <summary>Pescado, Crustáceo, Molusco, Cefalópodo, etc.</summary>
public class Categoria
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public bool Activo { get; set; } = true;

    public ICollection<Producto> Productos { get; set; } = new List<Producto>();
}

public class Producto
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? NombreCientifico { get; set; }
    /// <summary>Entero, Filete, Pelado, Cola, Pulpa...</summary>
    public string? Presentacion { get; set; }
    public UnidadMedida UnidadMedida { get; set; } = UnidadMedida.Kilogramo;
    public EstadoConservacion EstadoConservacion { get; set; }
    public decimal? TemperaturaMinima { get; set; }
    public decimal? TemperaturaMaxima { get; set; }
    public int DiasVidaUtil { get; set; }
    public decimal StockMinimo { get; set; }
    public decimal PrecioVenta { get; set; }
    /// <summary>Porcentaje de IVA: 0 para mariscos frescos/congelados en estado natural, 15 para procesados.</summary>
    public decimal TarifaIva { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; }

    public int CategoriaId { get; set; }
    public Categoria Categoria { get; set; } = null!;

    public ICollection<Lote> Lotes { get; set; } = new List<Lote>();
}

public class Proveedor
{
    public int Id { get; set; }
    public string Identificacion { get; set; } = string.Empty;
    public string RazonSocial { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public string? Direccion { get; set; }
    public bool Activo { get; set; } = true;

    public ICollection<Lote> Lotes { get; set; } = new List<Lote>();
}
