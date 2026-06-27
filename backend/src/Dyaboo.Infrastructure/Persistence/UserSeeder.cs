using Dyaboo.Domain.Entities;
using Dyaboo.Domain.Enums;
using Dyaboo.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace Dyaboo.Infrastructure.Persistence;

public static class UserSeeder
{
    public static async Task SeedAsync(DyabooDbContext db)
    {
        if (await db.Users.AnyAsync()) return;

        var hasher = new PasswordHasher();
        const string defaultPassword = "dyaboo2024";

        var users = new[]
        {
            User.Create("CEO Dyaboo",        "ceo@dyaboo.com",          hasher.Hash(defaultPassword), UserRole.Ceo),
            User.Create("Socio Dyaboo",      "socio@dyaboo.com",        hasher.Hash(defaultPassword), UserRole.Socio),
            User.Create("Líder PLM",         "plm@dyaboo.com",          hasher.Hash(defaultPassword), UserRole.LiderPlm),
            User.Create("Líder Producción",  "produccion@dyaboo.com",   hasher.Hash(defaultPassword), UserRole.LiderProduccion),
            User.Create("Líder Bodega",      "bodega@dyaboo.com",       hasher.Hash(defaultPassword), UserRole.LiderBodega),
            User.Create("Líder Distribución","distribucion@dyaboo.com", hasher.Hash(defaultPassword), UserRole.LiderDistribucion),
            User.Create("Diseñadora 1",      "disenadora@dyaboo.com",   hasher.Hash(defaultPassword), UserRole.Disenadora),
            User.Create("Vendedor 1",        "vendedor@dyaboo.com",     hasher.Hash(defaultPassword), UserRole.Vendedor),
            User.Create("Operario 1",        "operario@dyaboo.com",     hasher.Hash(defaultPassword), UserRole.Operario),
        };

        db.Users.AddRange(users);
        await db.SaveChangesAsync();
        Console.WriteLine($"[UserSeeder] {users.Length} usuarios creados. Contraseña: {defaultPassword}");
    }
}
