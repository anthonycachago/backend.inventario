using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventario.Web.Domain.Migrations
{
    /// <inheritdoc />
    public partial class ClientesVentasPagos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence(
                name: "VentaNumero",
                schema: "inv");

            migrationBuilder.AddColumn<decimal>(
                name: "PrecioVenta",
                schema: "inv",
                table: "Productos",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TarifaIva",
                schema: "inv",
                table: "Productos",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "VentaId",
                schema: "inv",
                table: "MovimientosInventario",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Clientes",
                schema: "inv",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TipoIdentificacion = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Identificacion = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RazonSocial = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Telefono = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Direccion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    LimiteCredito = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DiasCredito = table.Column<int>(type: "int", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clientes", x => x.Id);
                    table.CheckConstraint("CK_Clientes_ConsumidorFinal", "[TipoIdentificacion] <> 'ConsumidorFinal' OR [LimiteCredito] = 0");
                    table.CheckConstraint("CK_Clientes_Credito", "[LimiteCredito] >= 0 AND [DiasCredito] >= 0");
                });

            migrationBuilder.CreateTable(
                name: "Ventas",
                schema: "inv",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Numero = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CondicionPago = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FechaVencimiento = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Subtotal0 = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    SubtotalIva = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Iva = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Saldo = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Observacion = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    BodegaId = table.Column<int>(type: "int", nullable: true),
                    UsuarioId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ventas", x => x.Id);
                    table.CheckConstraint("CK_Ventas_Saldo", "[Saldo] >= 0 AND [Saldo] <= [Total]");
                    table.CheckConstraint("CK_Ventas_Total", "[Total] = [Subtotal0] + [SubtotalIva] + [Iva]");
                    table.CheckConstraint("CK_Ventas_Vencimiento", "([CondicionPago] = 'Contado' AND [FechaVencimiento] IS NULL) OR ([CondicionPago] = 'Credito' AND [FechaVencimiento] IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_Ventas_AspNetUsers_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Ventas_Bodegas_BodegaId",
                        column: x => x.BodegaId,
                        principalSchema: "inv",
                        principalTable: "Bodegas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Ventas_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalSchema: "inv",
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Pagos",
                schema: "inv",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Monto = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    FormaPago = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Referencia = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Banco = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TarjetaMarca = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TarjetaUltimos4 = table.Column<string>(type: "nchar(4)", fixedLength: true, maxLength: 4, nullable: true),
                    NumeroLote = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    NumeroAutorizacion = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Cuotas = table.Column<int>(type: "int", nullable: true),
                    Observacion = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Anulado = table.Column<bool>(type: "bit", nullable: false),
                    MotivoAnulacion = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    VentaId = table.Column<int>(type: "int", nullable: false),
                    UsuarioId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pagos", x => x.Id);
                    table.CheckConstraint("CK_Pagos_Cuotas", "[Cuotas] IS NULL OR [Cuotas] BETWEEN 1 AND 48");
                    table.CheckConstraint("CK_Pagos_Monto", "[Monto] > 0");
                    table.CheckConstraint("CK_Pagos_Tarjeta", "[FormaPago] NOT IN ('TarjetaCredito', 'TarjetaDebito') OR ([TarjetaMarca] IS NOT NULL AND [TarjetaUltimos4] LIKE '[0-9][0-9][0-9][0-9]' AND [NumeroAutorizacion] IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_Pagos_AspNetUsers_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Pagos_Ventas_VentaId",
                        column: x => x.VentaId,
                        principalSchema: "inv",
                        principalTable: "Ventas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VentaDetalles",
                schema: "inv",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Cantidad = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TarifaIva = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    VentaId = table.Column<int>(type: "int", nullable: false),
                    ProductoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VentaDetalles", x => x.Id);
                    table.CheckConstraint("CK_VentaDetalles_Valores", "[Cantidad] > 0 AND [PrecioUnitario] >= 0 AND [TarifaIva] >= 0");
                    table.ForeignKey(
                        name: "FK_VentaDetalles_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalSchema: "inv",
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VentaDetalles_Ventas_VentaId",
                        column: x => x.VentaId,
                        principalSchema: "inv",
                        principalTable: "Ventas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.AddCheckConstraint(
                name: "CK_Productos_PrecioVenta",
                schema: "inv",
                table: "Productos",
                sql: "[PrecioVenta] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Productos_TarifaIva",
                schema: "inv",
                table: "Productos",
                sql: "[TarifaIva] >= 0 AND [TarifaIva] <= 100");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosInventario_VentaId",
                schema: "inv",
                table: "MovimientosInventario",
                column: "VentaId");

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_Identificacion",
                schema: "inv",
                table: "Clientes",
                column: "Identificacion",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_Fecha",
                schema: "inv",
                table: "Pagos",
                column: "Fecha");

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_UsuarioId",
                schema: "inv",
                table: "Pagos",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_VentaId_Anulado",
                schema: "inv",
                table: "Pagos",
                columns: new[] { "VentaId", "Anulado" });

            migrationBuilder.CreateIndex(
                name: "IX_VentaDetalles_ProductoId",
                schema: "inv",
                table: "VentaDetalles",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_VentaDetalles_VentaId",
                schema: "inv",
                table: "VentaDetalles",
                column: "VentaId");

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_BodegaId",
                schema: "inv",
                table: "Ventas",
                column: "BodegaId");

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_ClienteId_Estado",
                schema: "inv",
                table: "Ventas",
                columns: new[] { "ClienteId", "Estado" });

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_Fecha",
                schema: "inv",
                table: "Ventas",
                column: "Fecha");

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_Numero",
                schema: "inv",
                table: "Ventas",
                column: "Numero",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_UsuarioId",
                schema: "inv",
                table: "Ventas",
                column: "UsuarioId");

            migrationBuilder.AddForeignKey(
                name: "FK_MovimientosInventario_Ventas_VentaId",
                schema: "inv",
                table: "MovimientosInventario",
                column: "VentaId",
                principalSchema: "inv",
                principalTable: "Ventas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MovimientosInventario_Ventas_VentaId",
                schema: "inv",
                table: "MovimientosInventario");

            migrationBuilder.DropTable(
                name: "Pagos",
                schema: "inv");

            migrationBuilder.DropTable(
                name: "VentaDetalles",
                schema: "inv");

            migrationBuilder.DropTable(
                name: "Ventas",
                schema: "inv");

            migrationBuilder.DropTable(
                name: "Clientes",
                schema: "inv");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Productos_PrecioVenta",
                schema: "inv",
                table: "Productos");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Productos_TarifaIva",
                schema: "inv",
                table: "Productos");

            migrationBuilder.DropIndex(
                name: "IX_MovimientosInventario_VentaId",
                schema: "inv",
                table: "MovimientosInventario");

            migrationBuilder.DropColumn(
                name: "PrecioVenta",
                schema: "inv",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "TarifaIva",
                schema: "inv",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "VentaId",
                schema: "inv",
                table: "MovimientosInventario");

            migrationBuilder.DropSequence(
                name: "VentaNumero",
                schema: "inv");
        }
    }
}
