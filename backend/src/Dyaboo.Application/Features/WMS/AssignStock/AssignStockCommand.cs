using MediatR;

namespace Dyaboo.Application.Features.WMS.AssignStock;

public record AssignStockCommand(
    Guid ProductReferenceId,
    IReadOnlyList<StockItemInput> Items) : IRequest<AssignStockResult>;

public record StockItemInput(Guid VariantId, int Quantity);

// ── Resultado ──────────────────────────────────────────────────────────────

public record AssignStockResult(
    Guid ProductReferenceId,
    string ProductName,
    int TotalUnitsAssigned,
    int LocationsUsed,
    IReadOnlyList<AssignmentDetail> Assignments);

public record AssignmentDetail(
    string LocationCode,
    string Aisle,
    int Shelf,
    int Level,
    string SKU,
    string Size,
    string Color,
    int QuantityAssigned,
    int LocationRemainingSpace);
