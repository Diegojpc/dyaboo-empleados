namespace Dyaboo.Domain.Entities;

/// <summary>Registro inmutable de cuántas unidades de una variante se colocaron en qué ubicación.</summary>
public class StockAssignment : BaseEntity
{
    public Guid WarehouseLocationId { get; private set; }
    public Guid ProductVariantId { get; private set; }
    public int Quantity { get; private set; }

    // Navegación EF
    public WarehouseLocation WarehouseLocation { get; private set; } = null!;
    public ProductVariant ProductVariant { get; private set; } = null!;

    private StockAssignment() { }

    public static StockAssignment Create(Guid locationId, Guid variantId, int quantity)
    {
        if (quantity <= 0) throw new ArgumentException("La cantidad asignada debe ser mayor a cero.");
        return new StockAssignment
        {
            WarehouseLocationId = locationId,
            ProductVariantId    = variantId,
            Quantity            = quantity
        };
    }
}
