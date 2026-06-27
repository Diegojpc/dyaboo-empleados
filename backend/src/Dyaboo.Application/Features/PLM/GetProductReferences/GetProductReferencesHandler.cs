using Dyaboo.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Dyaboo.Application.Features.PLM.GetProductReferences;

public class GetProductReferencesHandler(IApplicationDbContext db, IStorageService storage)
    : IRequestHandler<GetProductReferencesQuery, IReadOnlyList<ProductReferenceListItem>>
{
    public async Task<IReadOnlyList<ProductReferenceListItem>> Handle(
        GetProductReferencesQuery request,
        CancellationToken cancellationToken)
    {
        var refs = await db.ProductReferences
            .Where(p => p.IsActive)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new
            {
                p.Id, p.Name, p.ReferenceCode, p.Category,
                p.Description, p.IsActive, p.CreatedAt,
                Variants = p.Variants.Select(v => new VariantListItem(
                    v.Id, v.SKU, v.Size.Code, v.Color.Name, v.Color.HexCode,
                    v.CostPrice, v.StockQuantity)).ToList(),
                Images = p.Images.OrderBy(i => i.SortOrder)
                    .Select(i => new { i.Id, i.FileName, i.OriginalName, i.SortOrder })
                    .ToList(),
            })
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return refs.Select(p => new ProductReferenceListItem(
            p.Id,
            p.Name,
            p.ReferenceCode,
            p.Category.ToString(),
            p.Description,
            p.IsActive,
            p.Variants,
            p.Images.Select(i => new ImageListItem(
                i.Id,
                i.FileName,
                i.OriginalName,
                storage.GetPublicUrl(i.FileName),
                i.SortOrder)).ToList(),
            p.CreatedAt)).ToList();
    }
}
