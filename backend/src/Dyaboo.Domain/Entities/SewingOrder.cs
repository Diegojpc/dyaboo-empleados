using Dyaboo.Domain.Enums;

namespace Dyaboo.Domain.Entities;

/// <summary>
/// Orden de confección: unidades cortadas enviadas a un taller aliado (confeccionista).
/// Al recibirlas se registra el control de calidad (aprobadas/rechazadas).
/// </summary>
public class SewingOrder : BaseEntity
{
    public string OrderCode { get; private set; } = string.Empty;  // ej: OCF-20260701-A1B2C3
    public Guid CuttingOrderId { get; private set; }
    public Guid ConfeccionistaId { get; private set; }
    public SewingOrderStatus Status { get; private set; } = SewingOrderStatus.Assigned;
    public DateTime? ReceivedAt { get; private set; }

    private readonly List<SewingOrderItem> _items = [];
    public IReadOnlyCollection<SewingOrderItem> Items => _items.AsReadOnly();

    // Totales calculados (no mapeados en BD)
    public int TotalSent     => _items.Sum(i => i.QuantitySent);
    public int TotalApproved => _items.Sum(i => i.QuantityApproved);
    public int TotalRejected => _items.Sum(i => i.QuantityRejected);

    // Navegación EF
    public CuttingOrder CuttingOrder { get; private set; } = null!;
    public Confeccionista Confeccionista { get; private set; } = null!;

    private SewingOrder() { }

    public static SewingOrder Create(string orderCode, Guid cuttingOrderId, Guid confeccionistaId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(orderCode);

        return new SewingOrder
        {
            OrderCode        = orderCode.Trim().ToUpper(),
            CuttingOrderId   = cuttingOrderId,
            ConfeccionistaId = confeccionistaId
        };
    }

    public SewingOrderItem AddItem(Guid productVariantId, int quantitySent)
    {
        if (Status != SewingOrderStatus.Assigned)
            throw new InvalidOperationException("Solo se pueden agregar ítems a órdenes de confección asignadas.");

        var item = SewingOrderItem.Create(Id, productVariantId, quantitySent);
        _items.Add(item);
        MarkUpdated();
        return item;
    }

    /// <summary>
    /// Registra la recepción con control de calidad. Todo lo enviado debe
    /// contabilizarse: aprobadas + rechazadas = enviadas, por cada ítem.
    /// Las unidades aprobadas NO entran a stock aquí — se ubican después vía WMS.
    /// </summary>
    public void Receive(IReadOnlyDictionary<Guid, (int Approved, int Rejected)> receptionByItemId)
    {
        if (Status == SewingOrderStatus.Received)
            throw new InvalidOperationException("La orden de confección ya fue recibida.");

        foreach (var item in _items)
        {
            if (!receptionByItemId.TryGetValue(item.Id, out var reception))
                throw new InvalidOperationException(
                    $"Falta la recepción del ítem {item.Id}.");
            item.RegisterReception(reception.Approved, reception.Rejected);
        }

        Status     = SewingOrderStatus.Received;
        ReceivedAt = DateTime.UtcNow;
        MarkUpdated();
    }
}
