using Inventario.Web.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventario.Web.Domain.Configurations;

public class LoteConfiguration : IEntityTypeConfiguration<Lote>
{
    public void Configure(EntityTypeBuilder<Lote> builder)
    {
        builder.ToTable("Lotes", AppDbContext.Schema, t =>
        {
            t.HasCheckConstraint("CK_Lotes_Vencimiento", "CAST([FechaVencimiento] AS date) >= CAST([FechaIngreso] AS date)");
            t.HasCheckConstraint("CK_Lotes_CostoUnitario", "[CostoUnitario] >= 0");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Codigo).HasMaxLength(40).IsRequired();
        builder.Property(x => x.Origen).HasMaxLength(200);
        builder.Property(x => x.CertificadoSanitario).HasMaxLength(100);
        builder.Property(x => x.CostoUnitario).HasPrecision(18, 4);
        builder.HasIndex(x => new { x.ProductoId, x.Codigo }).IsUnique();
        // Consultas FEFO (primero en vencer, primero en salir)
        builder.HasIndex(x => x.FechaVencimiento);

        builder.HasOne(x => x.Producto)
            .WithMany(x => x.Lotes)
            .HasForeignKey(x => x.ProductoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Proveedor)
            .WithMany(x => x.Lotes)
            .HasForeignKey(x => x.ProveedorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class ExistenciaConfiguration : IEntityTypeConfiguration<Existencia>
{
    public void Configure(EntityTypeBuilder<Existencia> builder)
    {
        builder.ToTable("Existencias", AppDbContext.Schema, t =>
            t.HasCheckConstraint("CK_Existencias_Cantidad", "[Cantidad] >= 0"));
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Cantidad).HasPrecision(18, 3);
        // Concurrencia optimista: evita que dos usuarios descuenten el mismo stock a la vez
        builder.Property(x => x.RowVersion).IsRowVersion();
        builder.HasIndex(x => new { x.LoteId, x.UbicacionId }).IsUnique();

        builder.HasOne(x => x.Lote)
            .WithMany(x => x.Existencias)
            .HasForeignKey(x => x.LoteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Ubicacion)
            .WithMany(x => x.Existencias)
            .HasForeignKey(x => x.UbicacionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class MovimientoInventarioConfiguration : IEntityTypeConfiguration<MovimientoInventario>
{
    public void Configure(EntityTypeBuilder<MovimientoInventario> builder)
    {
        builder.ToTable("MovimientosInventario", AppDbContext.Schema, t =>
        {
            t.HasCheckConstraint("CK_MovimientosInventario_Cantidad", "[Cantidad] > 0");
            // Coherencia origen/destino según el tipo (el enum se guarda como texto).
            // Ajuste: positivo -> solo destino, negativo -> solo origen.
            t.HasCheckConstraint("CK_MovimientosInventario_Ubicaciones",
                "([Tipo] = 'Entrada' AND [UbicacionOrigenId] IS NULL AND [UbicacionDestinoId] IS NOT NULL) OR " +
                "([Tipo] IN ('Salida', 'Merma') AND [UbicacionOrigenId] IS NOT NULL AND [UbicacionDestinoId] IS NULL) OR " +
                "([Tipo] = 'Transferencia' AND [UbicacionOrigenId] IS NOT NULL AND [UbicacionDestinoId] IS NOT NULL AND [UbicacionOrigenId] <> [UbicacionDestinoId]) OR " +
                "([Tipo] = 'Ajuste' AND ([UbicacionOrigenId] IS NULL OR [UbicacionDestinoId] IS NULL) AND COALESCE([UbicacionOrigenId], [UbicacionDestinoId]) IS NOT NULL)");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Tipo).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.Cantidad).HasPrecision(18, 3);
        builder.Property(x => x.Referencia).HasMaxLength(50);
        builder.Property(x => x.Observacion).HasMaxLength(250);
        builder.Property(x => x.Fecha).HasDefaultValueSql("SYSUTCDATETIME()");
        builder.HasIndex(x => new { x.LoteId, x.Fecha });
        builder.HasIndex(x => x.Fecha);

        builder.HasOne(x => x.Lote)
            .WithMany(x => x.Movimientos)
            .HasForeignKey(x => x.LoteId)
            .OnDelete(DeleteBehavior.Restrict);

        // Dos FK a Ubicaciones: SQL Server no permite múltiples rutas en cascada
        builder.HasOne(x => x.UbicacionOrigen)
            .WithMany()
            .HasForeignKey(x => x.UbicacionOrigenId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.UbicacionDestino)
            .WithMany()
            .HasForeignKey(x => x.UbicacionDestinoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Usuario)
            .WithMany()
            .HasForeignKey(x => x.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Venta)
            .WithMany(x => x.Movimientos)
            .HasForeignKey(x => x.VentaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
