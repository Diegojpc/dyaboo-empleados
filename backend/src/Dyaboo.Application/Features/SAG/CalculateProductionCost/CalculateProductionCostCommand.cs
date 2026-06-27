using MediatR;

namespace Dyaboo.Application.Features.SAG.CalculateProductionCost;

public record CalculateProductionCostCommand(
    Guid ProductReferenceId,
    decimal OverheadPercentage,
    IReadOnlyList<ProductionItemInput> Items) : IRequest<ProductionCostResult>;

public record ProductionItemInput(
    Guid VariantId,
    int Quantity,
    decimal LaborCostPerUnit);

// ── Tipos de resultado ─────────────────────────────────────────────────────

public record ProductionCostResult(
    Guid OrderId,
    string OrderCode,
    string ProductReferenceName,
    string Currency,
    int TotalUnits,
    decimal OverheadPercentage,
    CostSummary Summary,
    IReadOnlyList<CostLineResult> Lines,
    DateTime CalculatedAt);

public record CostSummary(
    decimal TotalMaterialCost,
    decimal TotalLaborCost,
    decimal TotalOverheadCost,
    decimal GrandTotal,
    decimal CostPerUnit);

public record CostLineResult(
    Guid VariantId,
    string SKU,
    string Size,
    string Color,
    int Quantity,
    decimal MaterialCostPerUnit,
    decimal LaborCostPerUnit,
    decimal OverheadCostPerUnit,
    decimal TotalCostPerUnit,
    decimal TotalLineCost);
