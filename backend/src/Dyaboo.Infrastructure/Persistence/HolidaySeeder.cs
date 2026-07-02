using Dyaboo.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Dyaboo.Infrastructure.Persistence;

/// <summary>Siembra los festivos colombianos 2026-2027 (Ley Emiliani) si la tabla está vacía.</summary>
public static class HolidaySeeder
{
    public static async Task SeedAsync(DyabooDbContext db, CancellationToken ct = default)
    {
        if (await db.Holidays.AnyAsync(ct)) return;

        (int Year, int Month, int Day, string Name)[] festivos =
        [
            // 2026
            (2026,  1,  1, "Año Nuevo"),
            (2026,  1, 12, "Día de los Reyes Magos"),
            (2026,  3, 23, "Día de San José"),
            (2026,  4,  2, "Jueves Santo"),
            (2026,  4,  3, "Viernes Santo"),
            (2026,  5,  1, "Día del Trabajo"),
            (2026,  5, 18, "Ascensión del Señor"),
            (2026,  6,  8, "Corpus Christi"),
            (2026,  6, 15, "Sagrado Corazón de Jesús"),
            (2026,  6, 29, "San Pedro y San Pablo"),
            (2026,  7, 20, "Día de la Independencia"),
            (2026,  8,  7, "Batalla de Boyacá"),
            (2026,  8, 17, "Asunción de la Virgen"),
            (2026, 10, 12, "Día de la Raza"),
            (2026, 11,  2, "Todos los Santos"),
            (2026, 11, 16, "Independencia de Cartagena"),
            (2026, 12,  8, "Inmaculada Concepción"),
            (2026, 12, 25, "Navidad"),
            // 2027
            (2027,  1,  1, "Año Nuevo"),
            (2027,  1, 11, "Día de los Reyes Magos"),
            (2027,  3, 22, "Día de San José"),
            (2027,  3, 25, "Jueves Santo"),
            (2027,  3, 26, "Viernes Santo"),
            (2027,  5,  1, "Día del Trabajo"),
            (2027,  5, 10, "Ascensión del Señor"),
            (2027,  5, 31, "Corpus Christi"),
            (2027,  6,  7, "Sagrado Corazón de Jesús"),
            (2027,  7,  5, "San Pedro y San Pablo"),
            (2027,  7, 20, "Día de la Independencia"),
            (2027,  8,  7, "Batalla de Boyacá"),
            (2027,  8, 16, "Asunción de la Virgen"),
            (2027, 10, 18, "Día de la Raza"),
            (2027, 11,  1, "Todos los Santos"),
            (2027, 11, 15, "Independencia de Cartagena"),
            (2027, 12,  8, "Inmaculada Concepción"),
            (2027, 12, 25, "Navidad"),
        ];

        db.Holidays.AddRange(festivos.Select(f =>
            Holiday.Create(new DateOnly(f.Year, f.Month, f.Day), f.Name)));

        await db.SaveChangesAsync(ct);
        Console.WriteLine($"[HolidaySeeder] {festivos.Length} festivos 2026-2027 creados.");
    }
}
