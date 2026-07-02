using MediatR;

namespace Dyaboo.Application.Features.Produccion.GetCuttingOrders;

public record GetCuttingOrdersQuery : IRequest<IReadOnlyList<CuttingOrderResult>>;

public record CuttingOrderResult(
    Guid Id,
    string OrderCode,
    string ProductReferenceName,
    string Status,
    string? Notes,
    int TotalPlannedUnits,
    int TotalCutUnits,
    bool HasSewingOrder,
    IReadOnlyList<CuttingOrderItemResult> Items,
    DateTime CreatedAt,
    DateTime? CompletedAt);

public record CuttingOrderItemResult(
    Guid Id,
    string Sku,
    string Size,
    string Color,
    int PlannedQuantity,
    int CutQuantity);
