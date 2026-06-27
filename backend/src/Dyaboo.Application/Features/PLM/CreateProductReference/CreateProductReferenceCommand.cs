using Dyaboo.Domain.Enums;
using MediatR;

namespace Dyaboo.Application.Features.PLM.CreateProductReference;

public record CreateProductReferenceCommand(
    string Name,
    string ReferenceCode,
    ProductCategory Category,
    string Description,
    IReadOnlyList<VariantDto> Variants) : IRequest<ProductReferenceResult>;

public record VariantDto(
    string SizeCode,
    string ColorName,
    string ColorHex,
    string SKU,
    decimal CostPrice);

public record ProductReferenceResult(
    Guid Id,
    string Name,
    string ReferenceCode,
    string Category,
    IReadOnlyList<VariantResultDto> Variants,
    DateTime CreatedAt);

public record VariantResultDto(
    Guid Id,
    string SKU,
    string SizeCode,
    string ColorName,
    string ColorHex,
    decimal CostPrice);
