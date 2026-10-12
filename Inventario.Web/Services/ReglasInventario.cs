using Inventario.Web.Entity;

namespace Inventario.Web.Services;

/// <summary>Error de validación de negocio: el mensaje se muestra tal cual al usuario.</summary>
public class ReglaNegocioException(string message) : Exception(message);

public static class ReglasInventario
{
    /// <summary>Desviación permitida (°C) respecto a la temperatura objetivo de una ubicación.</summary>
    public const decimal ToleranciaTemperatura = 2m;

    /// <summary>Días antes del vencimiento a partir de los cuales un lote se marca "por vencer".</summary>
    public const int DiasAlertaVencimiento = 7;

    /// <summary>
    /// Indica si un producto puede almacenarse en un tipo de ubicación (cadena de frío).
    /// Andén y Cuarentena son zonas de tránsito y aceptan cualquier producto.
    /// </summary>
    public static bool EsCompatible(EstadoConservacion estado, TipoUbicacion tipo) => tipo switch
    {
        TipoUbicacion.Anden or TipoUbicacion.Cuarentena => true,
        TipoUbicacion.CamaraCongelacion => estado == EstadoConservacion.Congelado,
        TipoUbicacion.CamaraRefrigeracion => estado is EstadoConservacion.Fresco or EstadoConservacion.Refrigerado,
        TipoUbicacion.AreaSeca => estado == EstadoConservacion.Seco,
        _ => false
    };

    /// <summary>Factor para convertir a kg (null si la unidad no es de peso).</summary>
    public static decimal? FactorKg(UnidadMedida unidad) => unidad switch
    {
        UnidadMedida.Kilogramo => 1m,
        UnidadMedida.Libra => 0.45359237m,
        _ => null
    };

    public static bool TemperaturaFueraDeRango(decimal? objetivo, decimal? lectura) =>
        objetivo is not null && lectura is not null && Math.Abs(lectura.Value - objetivo.Value) > ToleranciaTemperatura;
}
