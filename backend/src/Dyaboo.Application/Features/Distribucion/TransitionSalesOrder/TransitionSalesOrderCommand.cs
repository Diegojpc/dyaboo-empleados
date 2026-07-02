using Dyaboo.Application.Features.Distribucion.GetSalesOrders;
using MediatR;

namespace Dyaboo.Application.Features.Distribucion.TransitionSalesOrder;

/// <summary>Transiciones simples del pedido que no mueven stock: Confirm, Deliver, Cancel.</summary>
public enum SalesOrderAction
{
    Confirm = 1,
    Deliver,
    Cancel
}

public record TransitionSalesOrderCommand(
    Guid SalesOrderId,
    SalesOrderAction Action) : IRequest<SalesOrderResult>;
