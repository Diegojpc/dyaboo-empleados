using Dyaboo.Application.Features.Produccion.GetCuttingOrders;
using Dyaboo.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Dyaboo.Application.Features.Produccion.CompleteCuttingOrder;

public class CompleteCuttingOrderHandler(IApplicationDbContext db)
    : IRequestHandler<CompleteCuttingOrderCommand, CuttingOrderResult>
{
    public async Task<CuttingOrderResult> Handle(
        CompleteCuttingOrderCommand request,
        CancellationToken cancellationToken)
    {
        var order = await db.CuttingOrders
            .Include(o => o.ProductReference)
            .Include(o => o.Items).ThenInclude(i => i.ProductVariant)
            .FirstOrDefaultAsync(o => o.Id == request.CuttingOrderId, cancellationToken)
            ?? throw new KeyNotFoundException($"Orden de corte '{request.CuttingOrderId}' no encontrada.");

        var cutByItemId = request.Items.ToDictionary(i => i.ItemId, i => i.CutQuantity);
        order.Complete(cutByItemId);

        await db.SaveChangesAsync(cancellationToken);

        return new CuttingOrderResult(
            order.Id, order.OrderCode, order.ProductReference.Name, order.Status.ToString(),
            order.Notes, order.TotalPlannedUnits, order.TotalCutUnits,
            HasSewingOrder: false,
            order.Items.Select(i => new CuttingOrderItemResult(
                i.Id,
                i.ProductVariant.SKU,
                i.ProductVariant.Size.Code,
                $"{i.ProductVariant.Color.Name} ({i.ProductVariant.Color.HexCode})",
                i.PlannedQuantity,
                i.CutQuantity)).ToList(),
            order.CreatedAt, order.CompletedAt);
    }
}
