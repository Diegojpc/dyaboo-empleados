using Dyaboo.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Dyaboo.Application.Features.SAG.GetFinancialInventory;

public class GetFinancialInventoryHandler(IApplicationDbContext db)
    : IRequestHandler<GetFinancialInventoryQuery, FinancialInventoryResult>
{
    public async Task<FinancialInventoryResult> Handle(
        GetFinancialInventoryQuery request,
        CancellationToken cancellationToken)
    {
        // Proyección directa — nunca Include() ciego en queries de lectura
        var lines = await db.ProductReferences
            .Where(p => p.IsActive)
            .Select(p => new FinancialInventoryLine(
                p.Id,
                p.ReferenceCode,
                p.Name,
                p.Variants.Sum(v => v.StockQuantity),
                p.Variants.Sum(v => v.StockQuantity * v.CostPrice),
                p.Variants.Select(v => new VariantFinancialDetail(
                    v.Id,
                    v.SKU,
                    v.Size.Code,
                    v.Color.Name + " (" + v.Color.HexCode + ")",
                    v.StockQuantity,
                    v.CostPrice,
                    v.StockQuantity * v.CostPrice
                )).ToList()
            ))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return new FinancialInventoryResult(
            "COP",
            lines.Sum(l => l.TotalValue),
            lines.Sum(l => l.Variants.Count),
            lines.Sum(l => l.TotalUnits),
            lines);
    }
}
