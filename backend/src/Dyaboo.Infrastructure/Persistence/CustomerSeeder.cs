using Dyaboo.Domain.Entities;
using Dyaboo.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Dyaboo.Infrastructure.Persistence;

/// <summary>Siembra clientes demo (tiendas propias y mayoristas) si la tabla está vacía.</summary>
public static class CustomerSeeder
{
    public static async Task SeedAsync(DyabooDbContext db, CancellationToken ct = default)
    {
        if (await db.Customers.AnyAsync(ct)) return;

        db.Customers.AddRange(
            Customer.Create("Tienda Dyaboo Centro", CustomerType.TiendaPropia,
                "Carolina Mesa", "3115550101", "Medellín"),
            Customer.Create("Tienda Dyaboo Envigado", CustomerType.TiendaPropia,
                "Andrés Cardona", "3115550102", "Envigado"),
            Customer.Create("Distribuidora Moda Bogotá", CustomerType.MayoristaExterno,
                "Paula Rincón", "3205550201", "Bogotá"),
            Customer.Create("Comercializadora El Hueco", CustomerType.MayoristaExterno,
                "Fernando Ruiz", "3205550202", "Medellín"));

        await db.SaveChangesAsync(ct);
    }
}
