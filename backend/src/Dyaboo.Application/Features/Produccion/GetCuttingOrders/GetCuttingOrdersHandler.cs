using Dyaboo.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Dyaboo.Application.Features.Produccion.GetCuttingOrders;

public class GetCuttingOrdersHandler(IApplicationDbContext db)
    : IRequestHandler<GetCuttingOrdersQuery, IReadOnlyList<CuttingOrderResult>>
{
    public async Task<IReadOnlyList<CuttingOrderResult>> Handle(
        GetCuttingOrdersQuery request,
        CancellationToken cancellationToken)
    {
        var orders = await db.CuttingOrders
            .AsNoTracking()
            .Include(o => o.ProductReference)
            .Include(o => o.Items).ThenInclude(i => i.ProductVariant)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);

        var withSewing = (await db.SewingOrders
            .AsNoTracking()
            .Select(s => s.CuttingOrderId)
            .ToListAsync(cancellationToken))
            .ToHashSet();

        return orders.Select(o => new CuttingOrderResult(
            o.Id,
            o.OrderCode,
            o.ProductReference.Name,
            o.Status.ToString(),
            o.Notes,
            o.TotalPlannedUnits,
            o.TotalCutUnits,
            withSewing.Contains(o.Id),
            o.Items.Select(i => new CuttingOrderItemResult(
                i.Id,
                i.ProductVariant.SKU,
                i.ProductVariant.Size.Code,
                $"{i.ProductVariant.Color.Name} ({i.ProductVariant.Color.HexCode})",
                i.PlannedQuantity,
                i.CutQuantity)).ToList(),
            o.CreatedAt,
            o.CompletedAt)).ToList();
    }
}
