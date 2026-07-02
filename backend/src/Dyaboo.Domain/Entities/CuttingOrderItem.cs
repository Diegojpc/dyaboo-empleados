namespace Dyaboo.Domain.Entities;

public class CuttingOrderItem : BaseEntity
{
    public Guid CuttingOrderId { get; private set; }
    public Guid ProductVariantId { get; private set; }
    public int PlannedQuantity { get; private set; }
    public int CutQuantity { get; private set; }  // 0 hasta que se completa el corte

    // Navegación EF
    public CuttingOrder CuttingOrder { get; private set; } = null!;
    public ProductVariant ProductVariant { get; private set; } = null!;

    private CuttingOrderItem() { }

    internal static CuttingOrderItem Create(Guid cuttingOrderId, Guid productVariantId, int plannedQuantity)
    {
        if (plannedQuantity <= 0)
            throw new ArgumentException("La cantidad planeada debe ser mayor a cero.");

        return new CuttingOrderItem
        {
            CuttingOrderId   = cuttingOrderId,
            ProductVariantId = productVariantId,
            PlannedQuantity  = plannedQuantity
        };
    }

    internal void RegisterCut(int cutQuantity)
    {
        if (cutQuantity < 0)
            throw new ArgumentException("La cantidad cortada no puede ser negativa.");
        CutQuantity = cutQuantity;
        MarkUpdated();
    }
}
