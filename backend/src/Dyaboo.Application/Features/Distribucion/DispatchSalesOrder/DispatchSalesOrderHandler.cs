using Dyaboo.Application.Interfaces;
using Dyaboo.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Dyaboo.Application.Features.Distribucion.DispatchSalesOrder;

/// <summary>
/// Despacha un pedido confirmado: consume el saldo de las asignaciones de bodega
/// en orden físico (Pasillo-Estante-Nivel, FIFO dentro de la ubicación), libera
/// espacio en las ubicaciones y descuenta el stock de cada variante.
/// </summary>
public class DispatchSalesOrderHandler(IApplicationDbContext db)
    : IRequestHandler<DispatchSalesOrderCommand, DispatchResult>
{
    public async Task<DispatchResult> Handle(
        DispatchSalesOrderCommand request,
        CancellationToken cancellationToken)
    {
        var order = await db.SalesOrders
            .Include(o => o.Items).ThenInclude(i => i.ProductVariant)
            .FirstOrDefaultAsync(o => o.Id == request.SalesOrderId, cancellationToken)
            ?? throw new KeyNotFoundException($"Pedido '{request.SalesOrderId}' no encontrado.");

        if (order.Status != SalesOrderStatus.Confirmed)
            throw new InvalidOperationException(
                $"Solo se pueden despachar pedidos confirmados (estado actual: {order.Status}).");

        var variantIds = order.Items.Select(i => i.ProductVariantId).ToList();

        // Asignaciones con saldo, en orden físico de recorrido de la bodega
        var assignments = await db.StockAssignments
            .Include(a => a.WarehouseLocation)
            .Where(a => variantIds.Contains(a.ProductVariantId) && a.RemainingQuantity > 0)
            .OrderBy(a => a.WarehouseLocation.LocationCode.Aisle)
            .ThenBy(a => a.WarehouseLocation.LocationCode.Shelf)
            .ThenBy(a => a.WarehouseLocation.LocationCode.Level)
            .ThenBy(a => a.CreatedAt)
            .ToListAsync(cancellationToken);

        var assignmentsByVariant = assignments
            .GroupBy(a => a.ProductVariantId)
            .ToDictionary(g => g.Key, g => g.ToList());

        // Pre-chequeo: nada se persiste si falta stock de alguna variante
        foreach (var item in order.Items)
        {
            var available = assignmentsByVariant.TryGetValue(item.ProductVariantId, out var list)
                ? list.Sum(a => a.RemainingQuantity)
                : 0;

            if (available < item.Quantity)
                throw new InvalidOperationException(
                    $"Stock insuficiente en bodega para {item.ProductVariant.SKU}: " +
                    $"solicitado {item.Quantity} u., disponible {available} u.");
        }

        var pickingLines = new List<PickingLine>();

        foreach (var item in order.Items)
        {
            int pending = item.Quantity;

            foreach (var assignment in assignmentsByVariant[item.ProductVariantId])
            {
                if (pending == 0) break;
                if (assignment.RemainingQuantity == 0) continue;

                var consumed = assignment.Consume(pending);
                assignment.WarehouseLocation.RemoveStock(consumed);
                pending -= consumed;

                pickingLines.Add(new PickingLine(
                    assignment.WarehouseLocation.LocationCode.Code,
                    item.ProductVariant.SKU,
                    consumed));
            }

            item.ProductVariant.AdjustStock(-item.Quantity);
        }

        order.MarkDispatched();
        await db.SaveChangesAsync(cancellationToken);

        return new DispatchResult(
            order.Id,
            order.OrderCode,
            order.TotalUnits,
            pickingLines);
    }
}
