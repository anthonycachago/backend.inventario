using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventario.Web.Domain.Migrations
{
    /// <inheritdoc />
    public partial class LocalizacionVistasYReglas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_Ubicaciones_Capacidad",
                schema: "inv",
                table: "Ubicaciones",
                sql: "[CapacidadKg] IS NULL OR [CapacidadKg] > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Productos_DiasVidaUtil",
                schema: "inv",
                table: "Productos",
                sql: "[DiasVidaUtil] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Productos_RangoTemperatura",
                schema: "inv",
                table: "Productos",
                sql: "[TemperaturaMinima] IS NULL OR [TemperaturaMaxima] IS NULL OR [TemperaturaMinima] <= [TemperaturaMaxima]");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Productos_StockMinimo",
                schema: "inv",
                table: "Productos",
                sql: "[StockMinimo] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_MovimientosInventario_Ubicaciones",
                schema: "inv",
                table: "MovimientosInventario",
                sql: "([Tipo] = 'Entrada' AND [UbicacionOrigenId] IS NULL AND [UbicacionDestinoId] IS NOT NULL) OR ([Tipo] IN ('Salida', 'Merma') AND [UbicacionOrigenId] IS NOT NULL AND [UbicacionDestinoId] IS NULL) OR ([Tipo] = 'Transferencia' AND [UbicacionOrigenId] IS NOT NULL AND [UbicacionDestinoId] IS NOT NULL AND [UbicacionOrigenId] <> [UbicacionDestinoId]) OR ([Tipo] = 'Ajuste' AND ([UbicacionOrigenId] IS NULL OR [UbicacionDestinoId] IS NULL) AND COALESCE([UbicacionOrigenId], [UbicacionDestinoId]) IS NOT NULL)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Lotes_CostoUnitario",
                schema: "inv",
                table: "Lotes",
                sql: "[CostoUnitario] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Lotes_Vencimiento",
                schema: "inv",
                table: "Lotes",
                sql: "CAST([FechaVencimiento] AS date) >= CAST([FechaIngreso] AS date)");

            // Vistas de reportes. EXEC(...) porque CREATE VIEW debe ir solo en su lote
            // y el script idempotente envuelve cada sentencia en un IF.
            CrearVista(migrationBuilder, VistaStockPorProducto);
            CrearVista(migrationBuilder, VistaStockPorUbicacion);
            CrearVista(migrationBuilder, VistaOcupacionUbicaciones);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW IF EXISTS [inv].[vw_OcupacionUbicaciones];");
            migrationBuilder.Sql("DROP VIEW IF EXISTS [inv].[vw_StockPorUbicacion];");
            migrationBuilder.Sql("DROP VIEW IF EXISTS [inv].[vw_StockPorProducto];");
            migrationBuilder.DropCheckConstraint(
                name: "CK_Ubicaciones_Capacidad",
                schema: "inv",
                table: "Ubicaciones");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Productos_DiasVidaUtil",
                schema: "inv",
                table: "Productos");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Productos_RangoTemperatura",
                schema: "inv",
                table: "Productos");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Productos_StockMinimo",
                schema: "inv",
                table: "Productos");

            migrationBuilder.DropCheckConstraint(
                name: "CK_MovimientosInventario_Ubicaciones",
                schema: "inv",
                table: "MovimientosInventario");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Lotes_CostoUnitario",
                schema: "inv",
                table: "Lotes");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Lotes_Vencimiento",
                schema: "inv",
                table: "Lotes");
        }

        private static void CrearVista(MigrationBuilder migrationBuilder, string sql) =>
            migrationBuilder.Sql($"EXEC(N'{sql.Replace("'", "''")}');");

        private const string VistaStockPorProducto = """
            CREATE OR ALTER VIEW [inv].[vw_StockPorProducto] AS
            SELECT p.Id AS ProductoId, p.Codigo, p.Nombre AS Producto, c.Nombre AS Categoria, p.UnidadMedida, p.StockMinimo,
                   CAST(ISNULL(SUM(e.Cantidad), 0) AS decimal(18,3)) AS StockTotal,
                   CAST(CASE WHEN ISNULL(SUM(e.Cantidad), 0) < p.StockMinimo THEN 1 ELSE 0 END AS bit) AS BajoMinimo
            FROM [inv].[Productos] p
            INNER JOIN [inv].[Categorias] c ON c.Id = p.CategoriaId
            LEFT JOIN [inv].[Lotes] l ON l.ProductoId = p.Id
            LEFT JOIN [inv].[Existencias] e ON e.LoteId = l.Id
            WHERE p.Activo = 1
            GROUP BY p.Id, p.Codigo, p.Nombre, c.Nombre, p.UnidadMedida, p.StockMinimo
            """;

        private const string VistaStockPorUbicacion = """
            CREATE OR ALTER VIEW [inv].[vw_StockPorUbicacion] AS
            SELECT e.Id AS ExistenciaId, b.Id AS BodegaId, b.Nombre AS Bodega,
                   u.Id AS UbicacionId, u.Codigo AS Ubicacion, u.Zona, u.Tipo AS TipoUbicacion,
                   l.Id AS LoteId, l.Codigo AS Lote, p.Id AS ProductoId, p.Nombre AS Producto, p.UnidadMedida,
                   e.Cantidad, l.CostoUnitario, l.FechaVencimiento,
                   DATEDIFF(DAY, CAST(GETDATE() AS date), CAST(l.FechaVencimiento AS date)) AS DiasParaVencer
            FROM [inv].[Existencias] e
            INNER JOIN [inv].[Lotes] l ON l.Id = e.LoteId
            INNER JOIN [inv].[Productos] p ON p.Id = l.ProductoId
            INNER JOIN [inv].[Ubicaciones] u ON u.Id = e.UbicacionId
            INNER JOIN [inv].[Bodegas] b ON b.Id = u.BodegaId
            WHERE e.Cantidad > 0
            """;

        // Ocupación en kg: convierte libras y omite unidades que no son de peso (unidad, caja).
        private const string VistaOcupacionUbicaciones = """
            CREATE OR ALTER VIEW [inv].[vw_OcupacionUbicaciones] AS
            SELECT u.Id AS UbicacionId, b.Id AS BodegaId, b.Nombre AS Bodega, u.Codigo, u.Zona, u.Pasillo, u.Rack, u.Nivel,
                   u.Tipo, u.Activo, u.CapacidadKg,
                   CAST(ISNULL(s.OcupadoKg, 0) AS decimal(18,3)) AS OcupadoKg,
                   CAST(CASE WHEN u.CapacidadKg > 0 THEN ISNULL(s.OcupadoKg, 0) * 100.0 / u.CapacidadKg END AS decimal(9,2)) AS PorcentajeOcupacion,
                   ISNULL(s.LotesDistintos, 0) AS LotesDistintos,
                   u.TemperaturaObjetivo,
                   t.Temperatura AS UltimaTemperatura,
                   t.FechaHora AS FechaUltimaTemperatura
            FROM [inv].[Ubicaciones] u
            INNER JOIN [inv].[Bodegas] b ON b.Id = u.BodegaId
            OUTER APPLY (
                SELECT SUM(e.Cantidad * CASE p.UnidadMedida WHEN 'Kilogramo' THEN 1.0 WHEN 'Libra' THEN 0.45359237 ELSE 0 END) AS OcupadoKg,
                       COUNT(DISTINCT e.LoteId) AS LotesDistintos
                FROM [inv].[Existencias] e
                INNER JOIN [inv].[Lotes] l ON l.Id = e.LoteId
                INNER JOIN [inv].[Productos] p ON p.Id = l.ProductoId
                WHERE e.UbicacionId = u.Id AND e.Cantidad > 0
            ) s
            OUTER APPLY (
                SELECT TOP (1) r.Temperatura, r.FechaHora
                FROM [inv].[RegistrosTemperatura] r
                WHERE r.UbicacionId = u.Id
                ORDER BY r.FechaHora DESC
            ) t
            """;
    }
}
