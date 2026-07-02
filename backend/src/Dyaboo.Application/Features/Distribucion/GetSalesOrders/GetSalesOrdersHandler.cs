using Dyaboo.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Dyaboo.Application.Features.Distribucion.GetSalesOrders;

public class GetSalesOrdersHandler(IApplicationDbContext db)
    : IRequestHandler<GetSalesOrdersQuery, IReadOnlyList<SalesOrderResult>>
{
    public async Task<IReadOnlyList<SalesOrderResult>> Handle(
        GetSalesOrdersQuery request,
        CancellationToken cancellationToken)
    {
        var orders = await db.SalesOrders
            .AsNoTracking()
            .Include(o => o.Customer)
            .Include(o => o.Items).ThenInclude(i => i.ProductVariant)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);

        return orders.Select(MapToResult).ToList();
    }

    internal static SalesOrderResult MapToResult(Domain.Entities.SalesOrder o) =>
        new(
            o.Id,
            o.OrderCode,
            o.Customer.Name,
            o.Customer.Type.ToString(),
            o.Status.ToString(),
            o.Notes,
            o.TotalUnits,
            o.Total,
            o.Items.Select(i => new SalesOrderItemResult(
                i.Id,
                i.ProductVariant.SKU,
                i.ProductVariant.Size.Code,
                $"{i.ProductVariant.Color.Name} ({i.ProductVariant.Color.HexCode})",
                i.Quantity,
                i.UnitPrice,
                i.LineTotal)).ToList(),
            o.CreatedAt,
            o.ConfirmedAt,
            o.DispatchedAt,
            o.DeliveredAt);
}
