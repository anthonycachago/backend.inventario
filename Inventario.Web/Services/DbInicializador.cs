using Inventario.Web.Domain;
using Inventario.Web.Entity;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Web.Services;

/// <summary>
/// Usuario técnico al que se atribuyen los movimientos mientras no exista login.
/// Cuando se integre la autenticación, reemplazar su uso por el usuario autenticado.
/// </summary>
public static class UsuarioSistema
{
    public const string Id = "00000000-0000-0000-0000-000000000001";
    public const string UserName = "sistema";
}

public static class DbInicializador
{
    public static async Task InicializarAsync(IServiceProvider services, bool aplicarMigraciones)
    {
        var factory = services.GetRequiredService<IDbContextFactory<AppDbContext>>();
        await using var db = await factory.CreateDbContextAsync();

        if (aplicarMigraciones)
            await db.Database.MigrateAsync();

        if (!await db.Users.AnyAsync(u => u.Id == UsuarioSistema.Id))
        {
            db.Users.Add(new ApplicationUser
            {
                Id = UsuarioSistema.Id,
                UserName = UsuarioSistema.UserName,
                NormalizedUserName = UsuarioSistema.UserName.ToUpperInvariant(),
                NombreCompleto = "Usuario del sistema",
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                LockoutEnabled = true
            });
            await db.SaveChangesAsync();
        }

        // Cliente genérico para ventas pequeñas sin datos del comprador
        if (!await db.Clientes.AnyAsync(c => c.Identificacion == Sri.IdentificacionEcuador.ConsumidorFinal))
        {
            db.Clientes.Add(new Cliente
            {
                TipoIdentificacion = TipoIdentificacion.ConsumidorFinal,
                Identificacion = Sri.IdentificacionEcuador.ConsumidorFinal,
                RazonSocial = "CONSUMIDOR FINAL"
            });
            await db.SaveChangesAsync();
        }
    }
}
