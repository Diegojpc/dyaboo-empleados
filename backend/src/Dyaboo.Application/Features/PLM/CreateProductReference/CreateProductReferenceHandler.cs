using Dyaboo.Application.Interfaces;
using Dyaboo.Domain.Entities;
using Dyaboo.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Dyaboo.Application.Features.PLM.CreateProductReference;

public class CreateProductReferenceHandler(IApplicationDbContext db)
    : IRequestHandler<CreateProductReferenceCommand, ProductReferenceResult>
{
    public async Task<ProductReferenceResult> Handle(
        CreateProductReferenceCommand request,
        CancellationToken cancellationToken)
    {
        var codeExists = await db.ProductReferences
            .AnyAsync(p => p.ReferenceCode == request.ReferenceCode.ToUpper(), cancellationToken);

        if (codeExists)
            throw new InvalidOperationException(
                $"Ya existe una referencia con el código '{request.ReferenceCode}'.");

        var product = ProductReference.Create(
            request.Name,
            request.ReferenceCode,
            request.Category,
            request.Description);

        foreach (var v in request.Variants)
        {
            var size = Size.From(v.SizeCode);
            var color = Color.From(v.ColorName, v.ColorHex);
            product.AddVariant(size, color, v.SKU, v.CostPrice);
        }

        db.ProductReferences.Add(product);
        await db.SaveChangesAsync(cancellationToken);

        return MapToResult(product);
    }

    private static ProductReferenceResult MapToResult(ProductReference p) =>
        new(
            p.Id,
            p.Name,
            p.ReferenceCode,
            p.Category.ToString(),
            p.Variants.Select(v => new VariantResultDto(
                v.Id, v.SKU, v.Size.Code, v.Color.Name, v.Color.HexCode, v.CostPrice
            )).ToList(),
            p.CreatedAt);
}
