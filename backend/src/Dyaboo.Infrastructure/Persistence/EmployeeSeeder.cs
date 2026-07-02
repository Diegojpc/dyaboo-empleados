using Dyaboo.Domain.Entities;
using Dyaboo.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Dyaboo.Infrastructure.Persistence;

/// <summary>
/// Siembra empleados demo cubriendo las tres direcciones del organigrama
/// de Alianza BSH, con cargos reales y fechas de ingreso variadas.
/// </summary>
public static class EmployeeSeeder
{
    public static async Task SeedAsync(DyabooDbContext db, CancellationToken ct = default)
    {
        if (await db.Employees.AnyAsync(ct)) return;

        var employees = new[]
        {
            // Dirección Comercial
            Employee.Create("Valentina Ochoa",   "1035860214", "Diseñadora",                CompanyArea.Diseno,     new DateOnly(2022,  3, 14), 3_200_000m, 44),
            Employee.Create("Sara Bedoya",       "1017234580", "Patronista",                CompanyArea.Diseno,     new DateOnly(2023,  8,  1), 2_600_000m, 44),
            Employee.Create("Camilo Herrera",    "1128054370", "Diseñador Gráfico",         CompanyArea.Mercadeo,   new DateOnly(2024,  2, 19), 2_400_000m, 44),
            Employee.Create("Manuela Cadavid",   "1152438906", "Asesora de ventas",         CompanyArea.Tiendas,    new DateOnly(2024, 10,  7), 1_623_500m, 44),
            Employee.Create("Julián Ospina",     "98624103",   "Asesor Comercial",          CompanyArea.Comercial,  new DateOnly(2023,  1, 30), 2_100_000m, 44),
            // Dirección de Operaciones
            Employee.Create("Albeiro Montoya",   "71356428",   "Jefe corte",                CompanyArea.Corte,      new DateOnly(2022,  1, 10), 3_500_000m, 44),
            Employee.Create("Diana Cifuentes",   "1040327615", "Auxiliar de corte",         CompanyArea.Corte,      new DateOnly(2025,  4, 21), 1_623_500m, 44),
            Employee.Create("Gloria Restrepo",   "43587210",   "Auditora de calidad",       CompanyArea.Produccion, new DateOnly(2023,  5,  2), 2_300_000m, 44),
            Employee.Create("Esteban Palacio",   "1036945872", "Auxiliar de bodega",        CompanyArea.Logistica,  new DateOnly(2024,  6,  3), 1_700_000m, 44),
            // Dirección Administrativa
            Employee.Create("Patricia Londoño",  "43096251",   "Directora financiera",      CompanyArea.Financiera, new DateOnly(2022,  2,  7), 8_000_000m, 44),
            Employee.Create("Andrés Zuluaga",    "1037612480", "Coordinador T.I",           CompanyArea.Proyectos,  new DateOnly(2023, 11, 13), 4_500_000m, 44),
            Employee.Create("Laura Betancur",    "1020459863", "Gestión humana y SST",      CompanyArea.Financiera, new DateOnly(2024,  1, 15), 3_000_000m, 44),
        };

        db.Employees.AddRange(employees);
        await db.SaveChangesAsync(ct);
        Console.WriteLine($"[EmployeeSeeder] {employees.Length} empleados demo creados.");
    }
}
