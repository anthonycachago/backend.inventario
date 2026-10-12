namespace Inventario.Web.Entity;

public enum UnidadMedida
{
    Kilogramo = 1,
    Libra = 2,
    Unidad = 3,
    Caja = 4
}

public enum EstadoConservacion
{
    Fresco = 1,
    Refrigerado = 2,
    Congelado = 3,
    Seco = 4
}

public enum TipoUbicacion
{
    CamaraCongelacion = 1,
    CamaraRefrigeracion = 2,
    AreaSeca = 3,
    Anden = 4,
    Cuarentena = 5
}

public enum TipoMovimiento
{
    Entrada = 1,
    Salida = 2,
    Transferencia = 3,
    Ajuste = 4,
    Merma = 5
}
