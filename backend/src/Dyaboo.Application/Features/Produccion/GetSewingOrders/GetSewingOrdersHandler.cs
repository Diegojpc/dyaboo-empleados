using Dyaboo.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Dyaboo.Application.Features.Produccion.GetSewingOrders;

public class GetSewingOrdersHandler(IApplicationDbContext db)
    : IRequestHandler<GetSewingOrdersQuery, IReadOnlyList<SewingOrderResult>>
{
    public async Task<IReadOnlyList<SewingOrderResult>> Handle(
        GetSewingOrdersQuery request,
        CancellationToken cancellationToken)
    {
        var orders = await db.SewingOrders
            .AsNoTracking()
            .Include(o => o.CuttingOrder)
            .Include(o => o.Confeccionista)
            .Include(o => o.Items).ThenInclude(i => i.ProductVariant)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);

        return orders.Select(MapToResult).ToList();
    }

    internal static SewingOrderResult MapToResult(Domain.Entities.SewingOrder o) =>
        new(
            o.Id,
            o.OrderCode,
            o.CuttingOrder.OrderCode,
            o.Confeccionista.Name,
            o.Status.ToString(),
            o.TotalSent,
            o.TotalApproved,
            o.TotalRejected,
            o.Items.Select(i => new SewingOrderItemResult(
                i.Id,
                i.ProductVariant.SKU,
                i.ProductVariant.Size.Code,
                $"{i.ProductVariant.Color.Name} ({i.ProductVariant.Color.HexCode})",
                i.QuantitySent,
                i.QuantityApproved,
                i.QuantityRejected)).ToList(),
            o.CreatedAt,
            o.ReceivedAt);
}
