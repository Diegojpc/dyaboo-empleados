using Dyaboo.Domain.Entities;
using Dyaboo.Domain.Enums;
using Dyaboo.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace Dyaboo.Infrastructure.Persistence;

/// <summary>
/// Crea el usuario de Gestión Humana. Guard por email (no por tabla vacía)
/// para que se aplique también en bases de datos existentes sin reset.
/// </summary>
public static class RrhhUserSeeder
{
    public static async Task SeedAsync(DyabooDbContext db, CancellationToken ct = default)
    {
        const string email = "rrhh@dyaboo.com";
        if (await db.Users.AnyAsync(u => u.Email == email, ct)) return;

        var hasher = new PasswordHasher();
        db.Users.Add(User.Create("Gestión Humana", email, hasher.Hash("dyaboo2024"), UserRole.GestionHumana));

        await db.SaveChangesAsync(ct);
        Console.WriteLine($"[RrhhUserSeeder] Usuario {email} creado (rol GestionHumana).");
    }
}
