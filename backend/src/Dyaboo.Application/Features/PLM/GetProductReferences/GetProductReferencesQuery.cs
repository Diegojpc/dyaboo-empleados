using MediatR;

namespace Dyaboo.Application.Features.PLM.GetProductReferences;

public record GetProductReferencesQuery : IRequest<IReadOnlyList<ProductReferenceListItem>>;

public record ProductReferenceListItem(
    Guid Id,
    string Name,
    string ReferenceCode,
    string Category,
    string Description,
    bool IsActive,
    IReadOnlyList<VariantListItem> Variants,
    IReadOnlyList<ImageListItem> Images,
    DateTime CreatedAt);

public record VariantListItem(
    Guid Id,
    string SKU,
    string SizeCode,
    string ColorName,
    string ColorHex,
    decimal CostPrice,
    int StockQuantity);

public record ImageListItem(
    Guid Id,
    string FileName,
    string OriginalName,
    string Url,
    int SortOrder);
