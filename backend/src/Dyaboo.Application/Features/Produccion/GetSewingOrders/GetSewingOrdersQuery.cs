using MediatR;

namespace Dyaboo.Application.Features.Produccion.GetSewingOrders;

public record GetSewingOrdersQuery : IRequest<IReadOnlyList<SewingOrderResult>>;

public record SewingOrderResult(
    Guid Id,
    string OrderCode,
    string CuttingOrderCode,
    string ConfeccionistaName,
    string Status,
    int TotalSent,
    int TotalApproved,
    int TotalRejected,
    IReadOnlyList<SewingOrderItemResult> Items,
    DateTime CreatedAt,
    DateTime? ReceivedAt);

public record SewingOrderItemResult(
    Guid Id,
    string Sku,
    string Size,
    string Color,
    int QuantitySent,
    int QuantityApproved,
    int QuantityRejected);
