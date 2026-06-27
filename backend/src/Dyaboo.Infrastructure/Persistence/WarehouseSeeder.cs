using Dyaboo.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Dyaboo.Infrastructure.Persistence;

/// <summary>Siembra ubicaciones físicas de bodega si la tabla está vacía.</summary>
public static class WarehouseSeeder
{
    // Layout: 3 pasillos × 5 estantes × 3 niveles = 45 ubicaciones, 300 u. c/u → 13.500 u. totales
    private static readonly string[] Aisles = ["A", "B", "C"];
    private const int Shelves  = 5;
    private const int Levels   = 3;
    private const int Capacity = 300;

    public static async Task SeedAsync(DyabooDbContext db, CancellationToken ct = default)
    {
        if (await db.WarehouseLocations.AnyAsync(ct)) return;

        var locations = new List<WarehouseLocation>();
        foreach (var aisle in Aisles)
            for (int shelf = 1; shelf <= Shelves; shelf++)
                for (int level = 1; level <= Levels; level++)
                    locations.Add(WarehouseLocation.Create(aisle, shelf, level, Capacity));

        db.WarehouseLocations.AddRange(locations);
        await db.SaveChangesAsync(ct);
    }
}
