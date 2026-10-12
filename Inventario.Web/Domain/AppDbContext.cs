using Inventario.Web.Entity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Web.Domain;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public const string Schema = "inv";

    // Catálogos
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Proveedor> Proveedores => Set<Proveedor>();

    // Localización
    public DbSet<Bodega> Bodegas => Set<Bodega>();
    public DbSet<Ubicacion> Ubicaciones => Set<Ubicacion>();
    public DbSet<RegistroTemperatura> RegistrosTemperatura => Set<RegistroTemperatura>();

    // Inventario
    public DbSet<Lote> Lotes => Set<Lote>();
    public DbSet<Existencia> Existencias => Set<Existencia>();
    public DbSet<MovimientoInventario> MovimientosInventario => Set<MovimientoInventario>();

    // Vistas (solo lectura)
    public DbSet<StockProductoVista> StockPorProducto => Set<StockProductoVista>();
    public DbSet<StockUbicacionVista> StockPorUbicacion => Set<StockUbicacionVista>();
    public DbSet<OcupacionUbicacionVista> OcupacionUbicaciones => Set<OcupacionUbicacionVista>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // Debe ir primero: configura las tablas de Identity (AspNetUsers, AspNetRoles...)
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(e =>
        {
            e.Property(x => x.NombreCompleto).HasMaxLength(150);
            e.Property(x => x.FechaCreacion).HasDefaultValueSql("SYSUTCDATETIME()");
        });

        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
