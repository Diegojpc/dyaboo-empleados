using Dyaboo.Application.Features.Distribucion.GetSalesOrders;
using MediatR;

namespace Dyaboo.Application.Features.Distribucion.CreateSalesOrder;

public record CreateSalesOrderCommand(
    Guid CustomerId,
    IReadOnlyList<SalesItemInput> Items,
    string? Notes) : IRequest<SalesOrderResult>;

public record SalesItemInput(Guid VariantId, int Quantity, decimal UnitPrice);
