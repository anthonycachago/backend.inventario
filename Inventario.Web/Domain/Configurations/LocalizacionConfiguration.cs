using Inventario.Web.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventario.Web.Domain.Configurations;

public class BodegaConfiguration : IEntityTypeConfiguration<Bodega>
{
    public void Configure(EntityTypeBuilder<Bodega> builder)
    {
        builder.ToTable("Bodegas", AppDbContext.Schema);
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Codigo).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Nombre).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Direccion).HasMaxLength(250);
        builder.Property(x => x.Ciudad).HasMaxLength(100);
        builder.Property(x => x.Latitud).HasPrecision(9, 6);
        builder.Property(x => x.Longitud).HasPrecision(9, 6);
        builder.HasIndex(x => x.Codigo).IsUnique();
    }
}

public class UbicacionConfiguration : IEntityTypeConfiguration<Ubicacion>
{
    public void Configure(EntityTypeBuilder<Ubicacion> builder)
    {
        builder.ToTable("Ubicaciones", AppDbContext.Schema, t =>
            t.HasCheckConstraint("CK_Ubicaciones_Capacidad", "[CapacidadKg] IS NULL OR [CapacidadKg] > 0"));
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Codigo).HasMaxLength(30).IsRequired();
        builder.Property(x => x.Tipo).HasConversion<string>().HasMaxLength(30);
        builder.Property(x => x.Zona).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Pasillo).HasMaxLength(10);
        builder.Property(x => x.Rack).HasMaxLength(10);
        builder.Property(x => x.Nivel).HasMaxLength(10);
        builder.Property(x => x.CapacidadKg).HasPrecision(18, 3);
        builder.Property(x => x.TemperaturaObjetivo).HasPrecision(5, 2);

        // El código es único dentro de cada bodega
        builder.HasIndex(x => new { x.BodegaId, x.Codigo }).IsUnique();

        builder.HasOne(x => x.Bodega)
            .WithMany(x => x.Ubicaciones)
            .HasForeignKey(x => x.BodegaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class RegistroTemperaturaConfiguration : IEntityTypeConfiguration<RegistroTemperatura>
{
    public void Configure(EntityTypeBuilder<RegistroTemperatura> builder)
    {
        builder.ToTable("RegistrosTemperatura", AppDbContext.Schema);
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Temperatura).HasPrecision(5, 2);
        builder.Property(x => x.Observacion).HasMaxLength(250);
        builder.HasIndex(x => new { x.UbicacionId, x.FechaHora });

        builder.HasOne(x => x.Ubicacion)
            .WithMany(x => x.RegistrosTemperatura)
            .HasForeignKey(x => x.UbicacionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Usuario)
            .WithMany()
            .HasForeignKey(x => x.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
