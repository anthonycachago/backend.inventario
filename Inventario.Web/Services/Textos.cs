using Inventario.Web.Entity;

namespace Inventario.Web.Services;

/// <summary>Nombres legibles de los enums para la interfaz.</summary>
public static class Textos
{
    public static string Nombre(this TipoUbicacion tipo) => tipo switch
    {
        TipoUbicacion.CamaraCongelacion => "Cámara de congelación",
        TipoUbicacion.CamaraRefrigeracion => "Cámara de refrigeración",
        TipoUbicacion.AreaSeca => "Área seca",
        TipoUbicacion.Anden => "Andén",
        TipoUbicacion.Cuarentena => "Cuarentena",
        _ => tipo.ToString()
    };

    public static string Nombre(this TipoMovimiento tipo) => tipo.ToString();

    public static string Abreviatura(this UnidadMedida unidad) => unidad switch
    {
        UnidadMedida.Kilogramo => "kg",
        UnidadMedida.Libra => "lb",
        UnidadMedida.Unidad => "u",
        UnidadMedida.Caja => "cj",
        _ => unidad.ToString()
    };

    /// <summary>Para columnas de vistas SQL donde el enum llega como texto.</summary>
    public static string Abreviatura(string unidad) =>
        Enum.TryParse<UnidadMedida>(unidad, out var u) ? u.Abreviatura() : unidad;

    public static string NombreTipoUbicacion(string tipo) =>
        Enum.TryParse<TipoUbicacion>(tipo, out var t) ? t.Nombre() : tipo;

    public static string ClaseBadge(this TipoMovimiento tipo) => tipo switch
    {
        TipoMovimiento.Entrada => "text-bg-success",
        TipoMovimiento.Salida => "text-bg-primary",
        TipoMovimiento.Transferencia => "text-bg-info",
        TipoMovimiento.Ajuste => "text-bg-warning",
        TipoMovimiento.Merma => "text-bg-danger",
        _ => "text-bg-secondary"
    };

    public static string ClaseBadge(this TipoUbicacion tipo) => tipo switch
    {
        TipoUbicacion.CamaraCongelacion => "text-bg-primary",
        TipoUbicacion.CamaraRefrigeracion => "text-bg-info",
        TipoUbicacion.AreaSeca => "text-bg-secondary",
        TipoUbicacion.Anden => "bg-body-secondary text-body border",
        TipoUbicacion.Cuarentena => "text-bg-danger",
        _ => "text-bg-secondary"
    };
}
