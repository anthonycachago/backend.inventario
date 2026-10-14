# backend.inventario

Sistema de inventario y localización para productos del mar (Blazor Server, .NET 10, EF Core, SQL Server).

## Funcionalidad

| Área | Vistas |
|---|---|
| Panel | Valor del inventario, lotes vencidos / por vencer, productos bajo mínimo, alertas de temperatura, ocupación por bodega, últimos movimientos |
| Operación | Recepción (entrada por lote), Despacho FEFO, Movimientos (salida, transferencia, ajuste por conteo, merma), Registro de temperaturas |
| Consultas | Existencias por ubicación, Lotes y trazabilidad, Kardex (por producto, tipo, fechas o lote), Mapa de bodega |
| Localización | Bodegas (con coordenadas), Ubicaciones (zona / pasillo / rack / nivel, capacidad y temperatura objetivo) |
| Ventas | Nueva venta (despacho FEFO + cobro), Ventas y cobros (abonos, anulación de pagos), Cuentas por cobrar por antigüedad, Clientes |
| Catálogos | Productos (precio e IVA), Categorías, Proveedores |

Modo claro / oscuro con el botón de la barra superior (se recuerda por navegador; por defecto sigue al sistema operativo).

## Reglas de negocio (`Services/InventarioService.cs`)

- Todo cambio de stock pasa por el servicio: actualiza `Existencias` y registra el movimiento en el kardex en una sola transacción.
- Cadena de frío: un producto congelado solo va a cámara de congelación; fresco/refrigerado a refrigeración; seco a área seca. Andén y cuarentena aceptan todo.
- Capacidad en kg por ubicación (convierte libras; unidades y cajas no cuentan peso).
- No se reciben lotes vencidos; un lote vencido solo sale como merma.
- Despacho FEFO: consume primero el lote que vence antes; ignora stock vencido y en cuarentena.
- Merma y ajuste exigen motivo; una lectura de temperatura fuera de ±2 °C exige acción correctiva.
- Concurrencia optimista sobre `Existencias` (RowVersion).
- Restricciones CHECK en base de datos como segunda barrera (cantidades, fechas, coherencia origen/destino por tipo de movimiento).

## Consulta SRI (`Services/Sri`)

- Validación local de cédula (módulo 10) y RUC (natural, pública, sociedad) antes de guardar.
- Botón **SRI** en clientes y proveedores: completa razón social y dirección de la matriz y avisa si el RUC no está
  activo o si el contribuyente está marcado como empresa fantasma o con transacciones inexistentes.
- Usa el catastro público de srienlinea.sri.gob.ec, que **no es una API documentada**. Si el SRI lo cambia o no
  responde, la app muestra el aviso y los datos se ingresan a mano. Resultados en caché 12 h.

## Ventas y cobros (`Services/VentasService.cs`)

- **Contado** ("de una"): los pagos deben cubrir el total; se admite pago mixto (efectivo + tarjeta, etc.).
- **Crédito** ("a cuenta"): requiere límite y días de crédito del cliente; bloquea si excede el límite o si tiene
  facturas vencidas. Abonos posteriores desde el detalle de la venta.
- **Tarjetas**: se cobran en el datáfono y se registra el voucher (marca, últimos 4 dígitos, autorización, lote,
  cuotas). El sistema **nunca** guarda el número completo, la expiración ni el CVV (restricción CHECK en la base).
- La venta despacha por FEFO en la misma transacción: si falta stock no se guarda nada. El kardex queda enlazado a la venta.
- IVA por producto (0 % mariscos en estado natural, 15 % procesados). Consumidor final máximo $50 por venta.
- Los pagos no se borran: se anulan con motivo y el monto vuelve al saldo.

## Base de datos

Scripts en `/Scripts` (ejecutar en orden):

1. `00_CrearBaseDatos.sql` — crea `InventarioMariscos` (en `master`).
2. `01_Esquema_Idempotente.sql` — tablas, índices, restricciones y vistas `inv.vw_StockPorProducto`, `inv.vw_StockPorUbicacion`, `inv.vw_OcupacionUbicaciones`. Re-ejecutable.
3. `02_DatosDemo.sql` — opcional: 2 bodegas, ubicaciones, proveedores, productos, lotes con kardex y lecturas de temperatura.

```bash
sqlcmd -S servidor -I -i Scripts/00_CrearBaseDatos.sql
sqlcmd -S servidor -d InventarioMariscos -I -f 65001 -i Scripts/01_Esquema_Idempotente.sql
sqlcmd -S servidor -d InventarioMariscos -I -f 65001 -i Scripts/02_DatosDemo.sql
```

Para regenerar el script de esquema después de una nueva migración:

```bash
cd Inventario.Web
dotnet ef migrations add NombreMigracion -o Domain/Migrations
dotnet ef migrations script --idempotent -o ../Scripts/01_Esquema_Idempotente.sql
```

(conservar la cabecera `SET QUOTED_IDENTIFIER ON` del archivo).

## Ejecutar

La cadena de conexión va en *user secrets* (no en `appsettings.json`):

```bash
cd Inventario.Web
dotnet user-secrets set "ConnectionStrings:Default" "Server=(localdb)\MSSQLLocalDB;Database=InventarioMariscos;Trusted_Connection=True;TrustServerCertificate=True"
dotnet run
```

En `Development` la app aplica las migraciones pendientes al iniciar. En otros entornos use los scripts.

## Pendiente

- Autenticación: los movimientos se atribuyen al usuario técnico `sistema` (`Services/DbInicializador.cs`). Identity ya está en el modelo; falta habilitar login y roles y usar el usuario autenticado.
- Facturación electrónica SRI (XML firmado y autorización): las ventas registran los valores, pero no emiten comprobante.
- Anulación de ventas con reingreso de stock.
- Cobro en línea con pasarela (Payphone, Kushki, etc.): requiere contrato de comercio y credenciales.
