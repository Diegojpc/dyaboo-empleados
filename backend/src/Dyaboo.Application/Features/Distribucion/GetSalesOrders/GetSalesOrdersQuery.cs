using MediatR;

namespace Dyaboo.Application.Features.Distribucion.GetSalesOrders;

public record GetSalesOrdersQuery : IRequest<IReadOnlyList<SalesOrderResult>>;

public record SalesOrderResult(
    Guid Id,
    string OrderCode,
    string CustomerName,
    string CustomerType,
    string Status,
    string? Notes,
    int TotalUnits,
    decimal Total,
    IReadOnlyList<SalesOrderItemResult> Items,
    DateTime CreatedAt,
    DateTime? ConfirmedAt,
    DateTime? DispatchedAt,
    DateTime? DeliveredAt);

public record SalesOrderItemResult(
    Guid Id,
    string Sku,
    string Size,
    string Color,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal);
