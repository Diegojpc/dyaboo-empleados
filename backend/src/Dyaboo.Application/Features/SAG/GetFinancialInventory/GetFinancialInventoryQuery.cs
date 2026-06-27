using MediatR;

namespace Dyaboo.Application.Features.SAG.GetFinancialInventory;

public record GetFinancialInventoryQuery : IRequest<FinancialInventoryResult>;

public record FinancialInventoryResult(
    string Currency,
    decimal GrandTotalValue,
    int TotalSkus,
    int TotalUnitsInStock,
    IReadOnlyList<FinancialInventoryLine> Lines);

public record FinancialInventoryLine(
    Guid ProductReferenceId,
    string ReferenceCode,
    string ProductName,
    int TotalUnits,
    decimal TotalValue,
    IReadOnlyList<VariantFinancialDetail> Variants);

public record VariantFinancialDetail(
    Guid VariantId,
    string SKU,
    string Size,
    string ColorName,
    string ColorHex,
    int StockQuantity,
    decimal UnitCost,
    decimal TotalValue);
