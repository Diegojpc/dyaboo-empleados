using Dyaboo.Application.Features.Produccion.GetSewingOrders;
using Dyaboo.Application.Interfaces;
using Dyaboo.Domain.Entities;
using Dyaboo.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Dyaboo.Application.Features.Produccion.CreateSewingOrder;

public class CreateSewingOrderHandler(IApplicationDbContext db)
    : IRequestHandler<CreateSewingOrderCommand, SewingOrderResult>
{
    public async Task<SewingOrderResult> Handle(
        CreateSewingOrderCommand request,
        CancellationToken cancellationToken)
    {
        var cuttingOrder = await db.CuttingOrders
            .Include(o => o.Items).ThenInclude(i => i.ProductVariant)
            .FirstOrDefaultAsync(o => o.Id == request.CuttingOrderId, cancellationToken)
            ?? throw new KeyNotFoundException($"Orden de corte '{request.CuttingOrderId}' no encontrada.");

        if (cuttingOrder.Status != CuttingOrderStatus.Completed)
            throw new InvalidOperationException(
                "Solo se puede enviar a confección una orden de corte completada.");

        var confeccionista = await db.Confeccionistas
            .FirstOrDefaultAsync(c => c.Id == request.ConfeccionistaId && c.IsActive, cancellationToken)
            ?? throw new KeyNotFoundException($"Confeccionista '{request.ConfeccionistaId}' no encontrado o inactivo.");

        var alreadySent = await db.SewingOrders
            .AnyAsync(s => s.CuttingOrderId == cuttingOrder.Id, cancellationToken);
        if (alreadySent)
            throw new InvalidOperationException(
                $"La orden de corte {cuttingOrder.OrderCode} ya fue enviada a confección.");

        var itemsToSend = cuttingOrder.Items.Where(i => i.CutQuantity > 0).ToList();
        if (itemsToSend.Count == 0)
            throw new InvalidOperationException(
                "La orden de corte no tiene unidades cortadas para enviar a confección.");

        var order = SewingOrder.Create(GenerateOrderCode(), cuttingOrder.Id, confeccionista.Id);
        foreach (var cutItem in itemsToSend)
            order.AddItem(cutItem.ProductVariantId, cutItem.CutQuantity);

        await db.SewingOrders.AddAsync(order, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        return new SewingOrderResult(
            order.Id, order.OrderCode, cuttingOrder.OrderCode, confeccionista.Name,
            order.Status.ToString(), order.TotalSent, order.TotalApproved, order.TotalRejected,
            order.Items.Select(i =>
            {
                var variant = itemsToSend.First(c => c.ProductVariantId == i.ProductVariantId).ProductVariant;
                return new SewingOrderItemResult(
                    i.Id, variant.SKU, variant.Size.Code,
                    $"{variant.Color.Name} ({variant.Color.HexCode})",
                    i.QuantitySent, i.QuantityApproved, i.QuantityRejected);
            }).ToList(),
            order.CreatedAt, order.ReceivedAt);
    }

    private static string GenerateOrderCode()
    {
        var now = DateTime.UtcNow;
        return $"OCF-{now:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}";
    }
}
