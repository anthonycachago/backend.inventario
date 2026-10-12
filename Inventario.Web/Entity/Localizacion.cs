namespace Inventario.Web.Entity;

public class Bodega
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Direccion { get; set; }
    public string? Ciudad { get; set; }
    public decimal? Latitud { get; set; }
    public decimal? Longitud { get; set; }
    public bool Activo { get; set; } = true;

    public ICollection<Ubicacion> Ubicaciones { get; set; } = new List<Ubicacion>();
}

/// <summary>Posición física dentro de una bodega. Ej: código "CC1-P02-R03-N1".</summary>
public class Ubicacion
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public TipoUbicacion Tipo { get; set; }
    /// <summary>Cámara o zona: CC1, CR2...</summary>
    public string Zona { get; set; } = string.Empty;
    public string? Pasillo { get; set; }
    public string? Rack { get; set; }
    public string? Nivel { get; set; }
    public decimal? CapacidadKg { get; set; }
    public decimal? TemperaturaObjetivo { get; set; }
    public bool Activo { get; set; } = true;

    public int BodegaId { get; set; }
    public Bodega Bodega { get; set; } = null!;

    public ICollection<Existencia> Existencias { get; set; } = new List<Existencia>();
    public ICollection<RegistroTemperatura> RegistrosTemperatura { get; set; } = new List<RegistroTemperatura>();
}

/// <summary>Control de cadena de frío por ubicación.</summary>
public class RegistroTemperatura
{
    public long Id { get; set; }
    public decimal Temperatura { get; set; }
    public DateTime FechaHora { get; set; }
    public string? Observacion { get; set; }

    public int UbicacionId { get; set; }
    public Ubicacion Ubicacion { get; set; } = null!;

    public string? UsuarioId { get; set; }
    public ApplicationUser? Usuario { get; set; }
}
