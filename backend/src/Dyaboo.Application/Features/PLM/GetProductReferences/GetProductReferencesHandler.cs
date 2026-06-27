using Dyaboo.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Dyaboo.Application.Features.PLM.GetProductReferences;

public class GetProductReferencesHandler(IApplicationDbContext db)
    : IRequestHandler<GetProductReferencesQuery, IReadOnlyList<ProductReferenceListItem>>
{
    public async Task<IReadOnlyList<ProductReferenceListItem>> Handle(
        GetProductReferencesQuery request,
        CancellationToken cancellationToken)
    {
        return await db.ProductReferences
            .Where(p => p.IsActive)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new ProductReferenceListItem(
                p.Id,
                p.Name,
                p.ReferenceCode,
                p.Category.ToString(),
                p.Description,
                p.IsActive,
                p.Variants.Select(v => new VariantListItem(
                    v.Id,
                    v.SKU,
                    v.Size.Code,
                    v.Color.Name,
                    v.Color.HexCode,
                    v.CostPrice,
                    v.StockQuantity
                )).ToList(),
                p.CreatedAt))
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}
