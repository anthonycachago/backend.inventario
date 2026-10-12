# backend.inventario

Sistema de inventario y localización para productos del mar (Blazor Server, .NET 10, EF Core, SQL Server).

## Funcionalidad

| Área | Vistas |
|---|---|
| Panel | Valor del inventario, lotes vencidos / por vencer, productos bajo mínimo, alertas de temperatura, ocupación por bodega, últimos movimientos |
| Operación | Recepción (entrada por lote), Despacho FEFO, Movimientos (salida, transferencia, ajuste por conteo, merma), Registro de temperaturas |
| Consultas | Existencias por ubicación, Lotes y trazabilidad, Kardex (por producto, tipo, fechas o lote), Mapa de bodega |
| Localización | Bodegas (con coordenadas), Ubicaciones (zona / pasillo / rack / nivel, capacidad y temperatura objetivo) |
| Catálogos | Productos, Categorías, Proveedores |

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
