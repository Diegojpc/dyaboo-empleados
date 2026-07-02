using Dyaboo.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Dyaboo.Infrastructure.Persistence;

/// <summary>Siembra talleres de confección aliados si la tabla está vacía.</summary>
public static class ConfeccionistaSeeder
{
    public static async Task SeedAsync(DyabooDbContext db, CancellationToken ct = default)
    {
        if (await db.Confeccionistas.AnyAsync(ct)) return;

        db.Confeccionistas.AddRange(
            Confeccionista.Create("Confecciones La 33", "Marta Zapata", "3104567890", "Medellín"),
            Confeccionista.Create("Taller Doña Rosa", "Rosa Álvarez", "3129876543", "Itagüí"),
            Confeccionista.Create("Manufacturas El Progreso", "Julián Restrepo", "3001234567", "Bello"));

        await db.SaveChangesAsync(ct);
    }
}
