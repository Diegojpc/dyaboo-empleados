using Dyaboo.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Dyaboo.Application.Features.WMS.GetWarehouseStatus;

public class GetWarehouseStatusHandler(IApplicationDbContext db)
    : IRequestHandler<GetWarehouseStatusQuery, WarehouseStatusResult>
{
    public async Task<WarehouseStatusResult> Handle(
        GetWarehouseStatusQuery request,
        CancellationToken cancellationToken)
    {
        // Proyección plana: una sola consulta con JOIN implícito vía EF
        var locations = await db.WarehouseLocations
            .Where(l => l.IsActive)
            .Select(l => new
            {
                l.Id,
                Aisle          = l.LocationCode.Aisle,
                Shelf          = l.LocationCode.Shelf,
                Level          = l.LocationCode.Level,
                Code           = l.LocationCode.Code,
                l.Capacity,
                l.CurrentStock,
                AvailableSpace = l.Capacity - l.CurrentStock,
                OccupancyPct   = l.Capacity > 0
                                    ? Math.Round((double)l.CurrentStock / l.Capacity * 100, 2)
                                    : 0.0,
                Skus = l.Id == l.Id  // fuerza inclusión del campo para el join posterior
                    ? db.StockAssignments
                        .Where(sa => sa.WarehouseLocationId == l.Id)
                        .Select(sa => sa.ProductVariant.SKU)
                        .Distinct()
                        .ToList()
                    : new List<string>()
            })
            .AsNoTracking()
            .OrderBy(l => l.Aisle).ThenBy(l => l.Shelf).ThenBy(l => l.Level)
            .ToListAsync(cancellationToken);

        var totalCapacity  = locations.Sum(l => l.Capacity);
        var totalStock     = locations.Sum(l => l.CurrentStock);
        var occupancyPct   = totalCapacity > 0
            ? Math.Round((double)totalStock / totalCapacity * 100, 2)
            : 0.0;

        var aisles = locations
            .GroupBy(l => l.Aisle)
            .Select(g => new AisleStatus(
                g.Key,
                g.Count(),
                g.Sum(l => l.Capacity),
                g.Sum(l => l.CurrentStock),
                g.Sum(l => l.Capacity) > 0
                    ? Math.Round((double)g.Sum(l => l.CurrentStock) / g.Sum(l => l.Capacity) * 100, 2)
                    : 0.0,
                g.Select(l => new LocationStatus(
                    l.Code, l.Shelf, l.Level,
                    l.Capacity, l.CurrentStock, l.AvailableSpace,
                    l.OccupancyPct, l.Skus))
                 .ToList()))
            .ToList();

        return new WarehouseStatusResult(
            locations.Count,
            locations.Count,
            locations.Count(l => l.CurrentStock > 0),
            totalCapacity,
            totalStock,
            occupancyPct,
            aisles);
    }
}
