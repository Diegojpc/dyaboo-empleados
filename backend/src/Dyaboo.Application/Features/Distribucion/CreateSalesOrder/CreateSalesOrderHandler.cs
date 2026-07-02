using Dyaboo.Application.Features.Distribucion.GetSalesOrders;
using Dyaboo.Application.Interfaces;
using Dyaboo.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Dyaboo.Application.Features.Distribucion.CreateSalesOrder;

public class CreateSalesOrderHandler(IApplicationDbContext db)
    : IRequestHandler<CreateSalesOrderCommand, SalesOrderResult>
{
    public async Task<SalesOrderResult> Handle(
        CreateSalesOrderCommand request,
        CancellationToken cancellationToken)
    {
        var customer = await db.Customers
            .FirstOrDefaultAsync(c => c.Id == request.CustomerId && c.IsActive, cancellationToken)
            ?? throw new KeyNotFoundException($"Cliente '{request.CustomerId}' no encontrado o inactivo.");

        var variantIds = request.Items.Select(i => i.VariantId).ToList();
        var variants = await db.ProductVariants
            .Where(v => variantIds.Contains(v.Id))
            .ToDictionaryAsync(v => v.Id, cancellationToken);

        var missing = variantIds.Where(id => !variants.ContainsKey(id)).ToList();
        if (missing.Count != 0)
            throw new InvalidOperationException(
                $"Variantes no encontradas: {string.Join(", ", missing)}");

        var order = SalesOrder.Create(GenerateOrderCode(), customer.Id, request.Notes);
        var itemResults = new List<SalesOrderItemResult>();

        foreach (var input in request.Items)
        {
            var variant = variants[input.VariantId];
            var item = order.AddItem(variant.Id, input.Quantity, input.UnitPrice);

            itemResults.Add(new SalesOrderItemResult(
                item.Id,
                variant.SKU,
                variant.Size.Code,
                $"{variant.Color.Name} ({variant.Color.HexCode})",
                item.Quantity,
                item.UnitPrice,
                item.LineTotal));
        }

        await db.SalesOrders.AddAsync(order, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        return new SalesOrderResult(
            order.Id, order.OrderCode, customer.Name, customer.Type.ToString(),
            order.Status.ToString(), order.Notes, order.TotalUnits, order.Total,
            itemResults, order.CreatedAt,
            order.ConfirmedAt, order.DispatchedAt, order.DeliveredAt);
    }

    private static string GenerateOrderCode()
    {
        var now = DateTime.UtcNow;
        return $"PED-{now:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}";
    }
}
