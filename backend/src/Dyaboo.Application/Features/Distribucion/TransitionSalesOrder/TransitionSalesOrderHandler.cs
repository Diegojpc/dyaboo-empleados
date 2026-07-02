using Dyaboo.Application.Features.Distribucion.GetSalesOrders;
using Dyaboo.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Dyaboo.Application.Features.Distribucion.TransitionSalesOrder;

public class TransitionSalesOrderHandler(IApplicationDbContext db)
    : IRequestHandler<TransitionSalesOrderCommand, SalesOrderResult>
{
    public async Task<SalesOrderResult> Handle(
        TransitionSalesOrderCommand request,
        CancellationToken cancellationToken)
    {
        var order = await db.SalesOrders
            .Include(o => o.Customer)
            .Include(o => o.Items).ThenInclude(i => i.ProductVariant)
            .FirstOrDefaultAsync(o => o.Id == request.SalesOrderId, cancellationToken)
            ?? throw new KeyNotFoundException($"Pedido '{request.SalesOrderId}' no encontrado.");

        switch (request.Action)
        {
            case SalesOrderAction.Confirm: order.Confirm();       break;
            case SalesOrderAction.Deliver: order.MarkDelivered(); break;
            case SalesOrderAction.Cancel:  order.Cancel();        break;
            default:
                throw new ArgumentException($"Acción desconocida: {request.Action}");
        }

        await db.SaveChangesAsync(cancellationToken);

        return GetSalesOrdersHandler.MapToResult(order);
    }
}
