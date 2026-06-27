using Dyaboo.Application.Interfaces;
using Dyaboo.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Dyaboo.Application.Features.WMS.AssignStock;

public class AssignStockHandler(IApplicationDbContext db)
    : IRequestHandler<AssignStockCommand, AssignStockResult>
{
    public async Task<AssignStockResult> Handle(
        AssignStockCommand request,
        CancellationToken cancellationToken)
    {
        var productRef = await db.ProductReferences
            .Include(p => p.Variants)
            .FirstOrDefaultAsync(p => p.Id == request.ProductReferenceId, cancellationToken)
            ?? throw new KeyNotFoundException($"Referencia '{request.ProductReferenceId}' no encontrada.");

        var variantMap = productRef.Variants.ToDictionary(v => v.Id);
        ValidateVariants(request.Items, variantMap);

        var totalRequested = request.Items.Sum(i => i.Quantity);

        // Carga ubicaciones disponibles con espacio, ordenadas para llenar secuencialmente
        var locations = await db.WarehouseLocations
            .Where(l => l.IsActive && l.CurrentStock < l.Capacity)
            .OrderBy(l => l.LocationCode.Aisle)
            .ThenBy(l => l.LocationCode.Shelf)
            .ThenBy(l => l.LocationCode.Level)
            .ToListAsync(cancellationToken);

        var totalAvailable = locations.Sum(l => l.AvailableSpace);
        if (totalAvailable < totalRequested)
            throw new InvalidOperationException(
                $"Capacidad insuficiente en bodega. Solicitado: {totalRequested} u. Disponible: {totalAvailable} u.");

        var assignments  = new List<StockAssignment>();
        var details      = new List<AssignmentDetail>();
        int locationIdx  = 0;

        foreach (var item in request.Items)
        {
            var variant   = variantMap[item.VariantId];
            int remaining = item.Quantity;

            while (remaining > 0)
            {
                // Avanza al próximo slot con espacio
                while (locationIdx < locations.Count && locations[locationIdx].IsFull)
                    locationIdx++;

                var location = locations[locationIdx];
                int assigned = location.Accommodate(remaining);  // actualiza CurrentStock en memoria
                remaining -= assigned;

                var sa = StockAssignment.Create(location.Id, variant.Id, assigned);
                assignments.Add(sa);

                details.Add(new AssignmentDetail(
                    location.LocationCode.Code,
                    location.LocationCode.Aisle,
                    location.LocationCode.Shelf,
                    location.LocationCode.Level,
                    variant.SKU,
                    variant.Size.Code,
                    $"{variant.Color.Name} ({variant.Color.HexCode})",
                    assigned,
                    location.AvailableSpace));
            }

            // Actualiza el stock de la variante (enlaza con el módulo SAG)
            variant.AdjustStock(item.Quantity);
        }

        db.StockAssignments.AddRange(assignments);
        await db.SaveChangesAsync(cancellationToken);

        return new AssignStockResult(
            productRef.Id,
            productRef.Name,
            totalRequested,
            details.Select(d => d.LocationCode).Distinct().Count(),
            details);
    }

    private static void ValidateVariants(
        IReadOnlyList<StockItemInput> items,
        Dictionary<Guid, ProductVariant> variantMap)
    {
        var invalid = items.Where(i => !variantMap.ContainsKey(i.VariantId)).ToList();
        if (invalid.Count != 0)
            throw new InvalidOperationException(
                $"Variantes no pertenecen a la referencia indicada: {string.Join(", ", invalid.Select(i => i.VariantId))}");

        var zeroes = items.Where(i => i.Quantity <= 0).ToList();
        if (zeroes.Count != 0)
            throw new ArgumentException("Todas las cantidades deben ser mayores a cero.");
    }
}
