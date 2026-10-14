using Inventario.Web.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventario.Web.Domain.Configurations;

// ToView hace que EF no genere tablas para estas entidades; las vistas se crean con SQL en la migración.

public class StockProductoVistaConfiguration : IEntityTypeConfiguration<StockProductoVista>
{
    public void Configure(EntityTypeBuilder<StockProductoVista> builder)
    {
        builder.HasNoKey().ToView("vw_StockPorProducto", AppDbContext.Schema);
        builder.Property(x => x.StockMinimo).HasPrecision(18, 3);
        builder.Property(x => x.StockTotal).HasPrecision(18, 3);
    }
}

public class StockUbicacionVistaConfiguration : IEntityTypeConfiguration<StockUbicacionVista>
{
    public void Configure(EntityTypeBuilder<StockUbicacionVista> builder)
    {
        builder.HasNoKey().ToView("vw_StockPorUbicacion", AppDbContext.Schema);
        builder.Property(x => x.Cantidad).HasPrecision(18, 3);
        builder.Property(x => x.CostoUnitario).HasPrecision(18, 4);
    }
}

public class OcupacionUbicacionVistaConfiguration : IEntityTypeConfiguration<OcupacionUbicacionVista>
{
    public void Configure(EntityTypeBuilder<OcupacionUbicacionVista> builder)
    {
        builder.HasNoKey().ToView("vw_OcupacionUbicaciones", AppDbContext.Schema);
        builder.Property(x => x.CapacidadKg).HasPrecision(18, 3);
        builder.Property(x => x.OcupadoKg).HasPrecision(18, 3);
        builder.Property(x => x.PorcentajeOcupacion).HasPrecision(9, 2);
        builder.Property(x => x.TemperaturaObjetivo).HasPrecision(5, 2);
        builder.Property(x => x.UltimaTemperatura).HasPrecision(5, 2);
    }
}
