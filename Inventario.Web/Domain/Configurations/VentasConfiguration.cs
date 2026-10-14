using Inventario.Web.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventario.Web.Domain.Configurations;

public class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> builder)
    {
        builder.ToTable("Clientes", AppDbContext.Schema, t =>
        {
            t.HasCheckConstraint("CK_Clientes_Credito", "[LimiteCredito] >= 0 AND [DiasCredito] >= 0");
            // El consumidor final (9999999999999) no puede comprar a crédito
            t.HasCheckConstraint("CK_Clientes_ConsumidorFinal", "[TipoIdentificacion] <> 'ConsumidorFinal' OR [LimiteCredito] = 0");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TipoIdentificacion).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.Identificacion).HasMaxLength(20).IsRequired();
        builder.Property(x => x.RazonSocial).HasMaxLength(300).IsRequired();
        builder.Property(x => x.Telefono).HasMaxLength(20);
        builder.Property(x => x.Email).HasMaxLength(150);
        builder.Property(x => x.Direccion).HasMaxLength(300);
        builder.Property(x => x.LimiteCredito).HasPrecision(18, 2);
        builder.HasIndex(x => x.Identificacion).IsUnique();
    }
}

public class VentaConfiguration : IEntityTypeConfiguration<Venta>
{
    public void Configure(EntityTypeBuilder<Venta> builder)
    {
        builder.ToTable("Ventas", AppDbContext.Schema, t =>
        {
            t.HasCheckConstraint("CK_Ventas_Saldo", "[Saldo] >= 0 AND [Saldo] <= [Total]");
            t.HasCheckConstraint("CK_Ventas_Total", "[Total] = [Subtotal0] + [SubtotalIva] + [Iva]");
            t.HasCheckConstraint("CK_Ventas_Vencimiento",
                "([CondicionPago] = 'Contado' AND [FechaVencimiento] IS NULL) OR ([CondicionPago] = 'Credito' AND [FechaVencimiento] IS NOT NULL)");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Numero).HasMaxLength(20).IsRequired();
        builder.Property(x => x.CondicionPago).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.Estado).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.Subtotal0).HasPrecision(18, 2);
        builder.Property(x => x.SubtotalIva).HasPrecision(18, 2);
        builder.Property(x => x.Iva).HasPrecision(18, 2);
        builder.Property(x => x.Total).HasPrecision(18, 2);
        builder.Property(x => x.Saldo).HasPrecision(18, 2);
        builder.Property(x => x.Observacion).HasMaxLength(250);
        // Concurrencia optimista: dos cajeros no pueden abonar a la vez sobre el mismo saldo
        builder.Property(x => x.RowVersion).IsRowVersion();
        builder.HasIndex(x => x.Numero).IsUnique();
        builder.HasIndex(x => x.Fecha);
        // Cartera: ventas con saldo por cliente
        builder.HasIndex(x => new { x.ClienteId, x.Estado });

        builder.HasOne(x => x.Cliente).WithMany(x => x.Ventas).HasForeignKey(x => x.ClienteId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Bodega).WithMany().HasForeignKey(x => x.BodegaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Usuario).WithMany().HasForeignKey(x => x.UsuarioId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class VentaDetalleConfiguration : IEntityTypeConfiguration<VentaDetalle>
{
    public void Configure(EntityTypeBuilder<VentaDetalle> builder)
    {
        builder.ToTable("VentaDetalles", AppDbContext.Schema, t =>
            t.HasCheckConstraint("CK_VentaDetalles_Valores", "[Cantidad] > 0 AND [PrecioUnitario] >= 0 AND [TarifaIva] >= 0"));
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Cantidad).HasPrecision(18, 3);
        builder.Property(x => x.PrecioUnitario).HasPrecision(18, 4);
        builder.Property(x => x.TarifaIva).HasPrecision(5, 2);
        builder.Property(x => x.Subtotal).HasPrecision(18, 2);

        builder.HasOne(x => x.Venta).WithMany(x => x.Detalles).HasForeignKey(x => x.VentaId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Producto).WithMany().HasForeignKey(x => x.ProductoId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class PagoConfiguration : IEntityTypeConfiguration<Pago>
{
    public void Configure(EntityTypeBuilder<Pago> builder)
    {
        builder.ToTable("Pagos", AppDbContext.Schema, t =>
        {
            t.HasCheckConstraint("CK_Pagos_Monto", "[Monto] > 0");
            // Tarjetas: datos del voucher obligatorios y solo 4 dígitos (nunca el número completo)
            t.HasCheckConstraint("CK_Pagos_Tarjeta",
                "[FormaPago] NOT IN ('TarjetaCredito', 'TarjetaDebito') OR " +
                "([TarjetaMarca] IS NOT NULL AND [TarjetaUltimos4] LIKE '[0-9][0-9][0-9][0-9]' AND [NumeroAutorizacion] IS NOT NULL)");
            t.HasCheckConstraint("CK_Pagos_Cuotas", "[Cuotas] IS NULL OR [Cuotas] BETWEEN 1 AND 48");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Monto).HasPrecision(18, 2);
        builder.Property(x => x.FormaPago).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.TarjetaMarca).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.TarjetaUltimos4).HasMaxLength(4).IsFixedLength();
        builder.Property(x => x.Referencia).HasMaxLength(50);
        builder.Property(x => x.Banco).HasMaxLength(100);
        builder.Property(x => x.NumeroLote).HasMaxLength(20);
        builder.Property(x => x.NumeroAutorizacion).HasMaxLength(20);
        builder.Property(x => x.Observacion).HasMaxLength(250);
        builder.Property(x => x.MotivoAnulacion).HasMaxLength(250);
        builder.HasIndex(x => new { x.VentaId, x.Anulado });
        builder.HasIndex(x => x.Fecha);

        builder.HasOne(x => x.Venta).WithMany(x => x.Pagos).HasForeignKey(x => x.VentaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Usuario).WithMany().HasForeignKey(x => x.UsuarioId).OnDelete(DeleteBehavior.Restrict);
    }
}
