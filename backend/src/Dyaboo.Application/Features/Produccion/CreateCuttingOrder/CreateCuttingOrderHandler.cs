using Dyaboo.Application.Features.Produccion.GetCuttingOrders;
using Dyaboo.Application.Interfaces;
using Dyaboo.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Dyaboo.Application.Features.Produccion.CreateCuttingOrder;

public class CreateCuttingOrderHandler(IApplicationDbContext db)
    : IRequestHandler<CreateCuttingOrderCommand, CuttingOrderResult>
{
    public async Task<CuttingOrderResult> Handle(
        CreateCuttingOrderCommand request,
        CancellationToken cancellationToken)
    {
        var productRef = await db.ProductReferences
            .Include(p => p.Variants)
            .FirstOrDefaultAsync(p => p.Id == request.ProductReferenceId && p.IsActive, cancellationToken)
            ?? throw new KeyNotFoundException($"Referencia '{request.ProductReferenceId}' no encontrada.");

        var variantMap = productRef.Variants.ToDictionary(v => v.Id);
        var invalid = request.Items.Where(i => !variantMap.ContainsKey(i.VariantId)).ToList();
        if (invalid.Count != 0)
            throw new InvalidOperationException(
                $"Variantes no pertenecen a la referencia indicada: {string.Join(", ", invalid.Select(i => i.VariantId))}");

        var order = CuttingOrder.Create(GenerateOrderCode(), productRef.Id, request.Notes);
        var items = new List<CuttingOrderItemResult>();

        foreach (var input in request.Items)
        {
            var variant = variantMap[input.VariantId];
            var item = order.AddItem(variant.Id, input.Quantity);

            items.Add(new CuttingOrderItemResult(
                item.Id,
                variant.SKU,
                variant.Size.Code,
                $"{variant.Color.Name} ({variant.Color.HexCode})",
                item.PlannedQuantity,
                item.CutQuantity));
        }

        await db.CuttingOrders.AddAsync(order, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        return new CuttingOrderResult(
            order.Id, order.OrderCode, productRef.Name, order.Status.ToString(),
            order.Notes, order.TotalPlannedUnits, order.TotalCutUnits,
            HasSewingOrder: false, items, order.CreatedAt, order.CompletedAt);
    }

    private static string GenerateOrderCode()
    {
        var now = DateTime.UtcNow;
        return $"OC-{now:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}";
    }
}
