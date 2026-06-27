using Dyaboo.Application.Interfaces;
using Dyaboo.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Dyaboo.Application.Features.SAG.CalculateProductionCost;

public class CalculateProductionCostHandler(IApplicationDbContext db)
    : IRequestHandler<CalculateProductionCostCommand, ProductionCostResult>
{
    public async Task<ProductionCostResult> Handle(
        CalculateProductionCostCommand request,
        CancellationToken cancellationToken)
    {
        var productRef = await db.ProductReferences
            .Include(p => p.Variants)
            .FirstOrDefaultAsync(p => p.Id == request.ProductReferenceId, cancellationToken)
            ?? throw new KeyNotFoundException($"Referencia de producto '{request.ProductReferenceId}' no encontrada.");

        ValidateVariantsBelongToReference(request, productRef);

        var orderCode = GenerateOrderCode();
        var order = ProductionOrder.Create(orderCode, productRef.Id, request.OverheadPercentage);

        var variantMap = productRef.Variants.ToDictionary(v => v.Id);
        var lineResults = new List<CostLineResult>();

        foreach (var input in request.Items)
        {
            var variant = variantMap[input.VariantId];
            var item = order.AddItem(
                variant.Id,
                input.Quantity,
                variant.CostPrice,
                input.LaborCostPerUnit);

            lineResults.Add(new CostLineResult(
                variant.Id,
                variant.SKU,
                variant.Size.Code,
                $"{variant.Color.Name} ({variant.Color.HexCode})",
                item.Quantity,
                item.MaterialCostPerUnit,
                item.LaborCostPerUnit,
                item.OverheadCostPerUnit,
                item.MaterialCostPerUnit + item.LaborCostPerUnit + item.OverheadCostPerUnit,
                item.TotalLineCost));
        }

        order.Approve();

        db.ProductionOrders.Add(order);
        await db.SaveChangesAsync(cancellationToken);

        var costPerUnit = order.TotalUnits > 0
            ? Math.Round(order.GrandTotal / order.TotalUnits, 4)
            : 0;

        return new ProductionCostResult(
            order.Id,
            order.OrderCode,
            productRef.Name,
            "COP",
            order.TotalUnits,
            order.OverheadPercentage,
            new CostSummary(
                order.TotalMaterialCost,
                order.TotalLaborCost,
                order.TotalOverheadCost,
                order.GrandTotal,
                costPerUnit),
            lineResults,
            order.CreatedAt);
    }

    private static void ValidateVariantsBelongToReference(
        CalculateProductionCostCommand request,
        ProductReference productRef)
    {
        var validIds = productRef.Variants.Select(v => v.Id).ToHashSet();
        var invalidIds = request.Items
            .Select(i => i.VariantId)
            .Where(id => !validIds.Contains(id))
            .ToList();

        if (invalidIds.Count != 0)
            throw new InvalidOperationException(
                $"Las siguientes variantes no pertenecen a la referencia indicada: {string.Join(", ", invalidIds)}");
    }

    private static string GenerateOrderCode()
    {
        var now = DateTime.UtcNow;
        return $"OP-{now:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}";
    }
}
