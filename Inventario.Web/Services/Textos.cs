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

    public static string Nombre(this FormaPago forma) => forma switch
    {
        FormaPago.Efectivo => "Efectivo",
        FormaPago.Transferencia => "Transferencia",
        FormaPago.Deposito => "Depósito",
        FormaPago.TarjetaCredito => "Tarjeta de crédito",
        FormaPago.TarjetaDebito => "Tarjeta de débito",
        FormaPago.Cheque => "Cheque",
        _ => forma.ToString()
    };

    public static string Icono(this FormaPago forma) => forma switch
    {
        FormaPago.Efectivo => "bi-cash-coin",
        FormaPago.Transferencia or FormaPago.Deposito => "bi-bank",
        FormaPago.TarjetaCredito or FormaPago.TarjetaDebito => "bi-credit-card",
        FormaPago.Cheque => "bi-receipt",
        _ => "bi-wallet2"
    };

    public static string Nombre(this MarcaTarjeta marca) => marca switch
    {
        MarcaTarjeta.AmericanExpress => "American Express",
        _ => marca.ToString()
    };

    public static string Nombre(this TipoIdentificacion tipo) => tipo switch
    {
        TipoIdentificacion.Cedula => "Cédula",
        TipoIdentificacion.Ruc => "RUC",
        TipoIdentificacion.Pasaporte => "Pasaporte",
        TipoIdentificacion.ConsumidorFinal => "Consumidor final",
        _ => tipo.ToString()
    };

    public static string Nombre(this CondicionPago condicion) =>
        condicion == CondicionPago.Contado ? "Contado" : "Crédito";

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
