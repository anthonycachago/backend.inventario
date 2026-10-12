/* =============================================================================
   00 - Creación de la base de datos
   Ejecutar conectado a [master]. Cambie el nombre si usa otro en la cadena de
   conexión (ConnectionStrings:Default).
   Orden: 00_CrearBaseDatos.sql -> 01_Esquema_Idempotente.sql -> 02_DatosDemo.sql
   ============================================================================= */
IF DB_ID(N'InventarioMariscos') IS NULL
BEGIN
    CREATE DATABASE [InventarioMariscos];
END
GO

-- Las reglas de concurrencia optimista (RowVersion) y la consistencia de lecturas
-- funcionan mejor con READ_COMMITTED_SNAPSHOT (evita bloqueos lectores/escritores).
ALTER DATABASE [InventarioMariscos] SET READ_COMMITTED_SNAPSHOT ON WITH ROLLBACK IMMEDIATE;
GO
