using Dyaboo.Domain.Enums;

namespace Dyaboo.Domain.Entities;

/// <summary>Orden de corte interna: cuántas unidades de cada variante se planea cortar.</summary>
public class CuttingOrder : BaseEntity
{
    public string OrderCode { get; private set; } = string.Empty;  // ej: OC-20260701-A1B2C3
    public Guid ProductReferenceId { get; private set; }
    public CuttingOrderStatus Status { get; private set; } = CuttingOrderStatus.InProgress;
    public string? Notes { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    private readonly List<CuttingOrderItem> _items = [];
    public IReadOnlyCollection<CuttingOrderItem> Items => _items.AsReadOnly();

    // Totales calculados (no mapeados en BD)
    public int TotalPlannedUnits => _items.Sum(i => i.PlannedQuantity);
    public int TotalCutUnits     => _items.Sum(i => i.CutQuantity);

    // Navegación EF
    public ProductReference ProductReference { get; private set; } = null!;

    private CuttingOrder() { }

    public static CuttingOrder Create(string orderCode, Guid productReferenceId, string? notes = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(orderCode);

        return new CuttingOrder
        {
            OrderCode          = orderCode.Trim().ToUpper(),
            ProductReferenceId = productReferenceId,
            Notes              = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim()
        };
    }

    public CuttingOrderItem AddItem(Guid productVariantId, int plannedQuantity)
    {
        if (Status != CuttingOrderStatus.InProgress)
            throw new InvalidOperationException("Solo se pueden agregar ítems a órdenes de corte en proceso.");

        var item = CuttingOrderItem.Create(Id, productVariantId, plannedQuantity);
        _items.Add(item);
        MarkUpdated();
        return item;
    }

    /// <summary>
    /// Registra las unidades realmente cortadas por ítem (puede diferir de lo planeado
    /// por mermas de tela) y cierra la orden.
    /// </summary>
    public void Complete(IReadOnlyDictionary<Guid, int> cutQuantitiesByItemId)
    {
        if (Status == CuttingOrderStatus.Completed)
            throw new InvalidOperationException("La orden de corte ya fue completada.");
        if (_items.Count == 0)
            throw new InvalidOperationException("No se puede completar una orden de corte sin ítems.");

        foreach (var item in _items)
        {
            if (!cutQuantitiesByItemId.TryGetValue(item.Id, out var cutQuantity))
                throw new InvalidOperationException(
                    $"Falta la cantidad cortada del ítem {item.Id}.");
            item.RegisterCut(cutQuantity);
        }

        Status      = CuttingOrderStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        MarkUpdated();
    }
}
