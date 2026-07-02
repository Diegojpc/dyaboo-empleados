using Dyaboo.Application.Features.Produccion.GetSewingOrders;
using Dyaboo.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Dyaboo.Application.Features.Produccion.ReceiveSewingOrder;

public class ReceiveSewingOrderHandler(IApplicationDbContext db)
    : IRequestHandler<ReceiveSewingOrderCommand, SewingOrderResult>
{
    public async Task<SewingOrderResult> Handle(
        ReceiveSewingOrderCommand request,
        CancellationToken cancellationToken)
    {
        var order = await db.SewingOrders
            .Include(o => o.CuttingOrder)
            .Include(o => o.Confeccionista)
            .Include(o => o.Items).ThenInclude(i => i.ProductVariant)
            .FirstOrDefaultAsync(o => o.Id == request.SewingOrderId, cancellationToken)
            ?? throw new KeyNotFoundException($"Orden de confección '{request.SewingOrderId}' no encontrada.");

        var receptionByItemId = request.Items.ToDictionary(
            i => i.ItemId,
            i => (i.Approved, i.Rejected));

        order.Receive(receptionByItemId);

        await db.SaveChangesAsync(cancellationToken);

        return GetSewingOrdersHandler.MapToResult(order);
    }
}
