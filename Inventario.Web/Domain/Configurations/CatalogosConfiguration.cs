using Inventario.Web.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventario.Web.Domain.Configurations;

public class CategoriaConfiguration : IEntityTypeConfiguration<Categoria>
{
    public void Configure(EntityTypeBuilder<Categoria> builder)
    {
        builder.ToTable("Categorias", AppDbContext.Schema);
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Nombre).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Descripcion).HasMaxLength(250);
        builder.HasIndex(x => x.Nombre).IsUnique();

        builder.HasData(
            new Categoria { Id = 1, Nombre = "Pescado" },
            new Categoria { Id = 2, Nombre = "Crustáceo" },
            new Categoria { Id = 3, Nombre = "Molusco" },
            new Categoria { Id = 4, Nombre = "Cefalópodo" });
    }
}

public class ProductoConfiguration : IEntityTypeConfiguration<Producto>
{
    public void Configure(EntityTypeBuilder<Producto> builder)
    {
        builder.ToTable("Productos", AppDbContext.Schema, t =>
        {
            t.HasCheckConstraint("CK_Productos_StockMinimo", "[StockMinimo] >= 0");
            t.HasCheckConstraint("CK_Productos_DiasVidaUtil", "[DiasVidaUtil] >= 0");
            t.HasCheckConstraint("CK_Productos_RangoTemperatura",
                "[TemperaturaMinima] IS NULL OR [TemperaturaMaxima] IS NULL OR [TemperaturaMinima] <= [TemperaturaMaxima]");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Codigo).HasMaxLength(30).IsRequired();
        builder.Property(x => x.Nombre).HasMaxLength(150).IsRequired();
        builder.Property(x => x.NombreCientifico).HasMaxLength(150);
        builder.Property(x => x.Presentacion).HasMaxLength(50);
        builder.Property(x => x.UnidadMedida).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.EstadoConservacion).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.TemperaturaMinima).HasPrecision(5, 2);
        builder.Property(x => x.TemperaturaMaxima).HasPrecision(5, 2);
        builder.Property(x => x.StockMinimo).HasPrecision(18, 3);
        builder.Property(x => x.FechaCreacion).HasDefaultValueSql("SYSUTCDATETIME()");
        builder.HasIndex(x => x.Codigo).IsUnique();

        builder.HasOne(x => x.Categoria)
            .WithMany(x => x.Productos)
            .HasForeignKey(x => x.CategoriaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class ProveedorConfiguration : IEntityTypeConfiguration<Proveedor>
{
    public void Configure(EntityTypeBuilder<Proveedor> builder)
    {
        builder.ToTable("Proveedores", AppDbContext.Schema);
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Identificacion).HasMaxLength(20).IsRequired();
        builder.Property(x => x.RazonSocial).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Telefono).HasMaxLength(20);
        builder.Property(x => x.Email).HasMaxLength(150);
        builder.Property(x => x.Direccion).HasMaxLength(250);
        builder.HasIndex(x => x.Identificacion).IsUnique();
    }
}
