using Dyaboo.Domain.Enums;

namespace Dyaboo.Domain.Entities;

/// <summary>
/// Pedido de distribución. Máquina de estados:
/// Draft → Confirmed → Dispatched → Delivered; Cancelled solo desde Draft/Confirmed.
/// </summary>
public class SalesOrder : BaseEntity
{
    public string OrderCode { get; private set; } = string.Empty;  // ej: PED-20260701-A1B2C3
    public Guid CustomerId { get; private set; }
    public SalesOrderStatus Status { get; private set; } = SalesOrderStatus.Draft;
    public string? Notes { get; private set; }
    public DateTime? ConfirmedAt { get; private set; }
    public DateTime? DispatchedAt { get; private set; }
    public DateTime? DeliveredAt { get; private set; }

    private readonly List<SalesOrderItem> _items = [];
    public IReadOnlyCollection<SalesOrderItem> Items => _items.AsReadOnly();

    // Totales calculados (no mapeados en BD)
    public decimal Total      => _items.Sum(i => i.LineTotal);
    public int     TotalUnits => _items.Sum(i => i.Quantity);

    // Navegación EF
    public Customer Customer { get; private set; } = null!;

    private SalesOrder() { }

    public static SalesOrder Create(string orderCode, Guid customerId, string? notes = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(orderCode);

        return new SalesOrder
        {
            OrderCode  = orderCode.Trim().ToUpper(),
            CustomerId = customerId,
            Notes      = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim()
        };
    }

    public SalesOrderItem AddItem(Guid productVariantId, int quantity, decimal unitPrice)
    {
        if (Status != SalesOrderStatus.Draft)
            throw new InvalidOperationException("Solo se pueden agregar ítems a pedidos en borrador.");

        var item = SalesOrderItem.Create(Id, productVariantId, quantity, unitPrice);
        _items.Add(item);
        MarkUpdated();
        return item;
    }

    public void Confirm()
    {
        if (Status != SalesOrderStatus.Draft)
            throw new InvalidOperationException($"Solo se puede confirmar un pedido en borrador (estado actual: {Status}).");
        if (_items.Count == 0)
            throw new InvalidOperationException("No se puede confirmar un pedido sin ítems.");

        Status      = SalesOrderStatus.Confirmed;
        ConfirmedAt = DateTime.UtcNow;
        MarkUpdated();
    }

    public void MarkDispatched()
    {
        if (Status != SalesOrderStatus.Confirmed)
            throw new InvalidOperationException($"Solo se puede despachar un pedido confirmado (estado actual: {Status}).");

        Status       = SalesOrderStatus.Dispatched;
        DispatchedAt = DateTime.UtcNow;
        MarkUpdated();
    }

    public void MarkDelivered()
    {
        if (Status != SalesOrderStatus.Dispatched)
            throw new InvalidOperationException($"Solo se puede entregar un pedido despachado (estado actual: {Status}).");

        Status      = SalesOrderStatus.Delivered;
        DeliveredAt = DateTime.UtcNow;
        MarkUpdated();
    }

    public void Cancel()
    {
        if (Status is not (SalesOrderStatus.Draft or SalesOrderStatus.Confirmed))
            throw new InvalidOperationException($"Solo se pueden cancelar pedidos en borrador o confirmados (estado actual: {Status}).");

        Status = SalesOrderStatus.Cancelled;
        MarkUpdated();
    }
}
