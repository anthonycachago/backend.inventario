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

public enum TipoIdentificacion
{
    Cedula = 1,
    Ruc = 2,
    Pasaporte = 3,
    ConsumidorFinal = 4
}

public enum CondicionPago
{
    /// <summary>Se paga "de una": los pagos deben cubrir el total al registrar la venta.</summary>
    Contado = 1,
    /// <summary>"A cuenta": queda saldo por cobrar con abonos.</summary>
    Credito = 2
}

public enum EstadoVenta
{
    Pendiente = 1,
    Pagada = 2
}

public enum FormaPago
{
    Efectivo = 1,
    Transferencia = 2,
    Deposito = 3,
    TarjetaCredito = 4,
    TarjetaDebito = 5,
    Cheque = 6
}

public enum MarcaTarjeta
{
    Visa = 1,
    Mastercard = 2,
    AmericanExpress = 3,
    Diners = 4,
    Discover = 5,
    Otra = 6
}
