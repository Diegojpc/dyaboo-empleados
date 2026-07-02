namespace Dyaboo.Domain.Entities;

/// <summary>
/// Registro de cuántas unidades de una variante se colocaron en qué ubicación.
/// Quantity conserva lo asignado originalmente; RemainingQuantity es el saldo
/// despachable que se consume al despachar pedidos.
/// </summary>
public class StockAssignment : BaseEntity
{
    public Guid WarehouseLocationId { get; private set; }
    public Guid ProductVariantId { get; private set; }
    public int Quantity { get; private set; }
    public int RemainingQuantity { get; private set; }

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
            Quantity            = quantity,
            RemainingQuantity   = quantity
        };
    }

    /// <summary>Consume hasta 'requested' unidades del saldo; devuelve lo realmente consumido.</summary>
    public int Consume(int requested)
    {
        if (requested <= 0)
            throw new ArgumentException("La cantidad a consumir debe ser mayor a cero.");

        var consumed = Math.Min(requested, RemainingQuantity);
        RemainingQuantity -= consumed;
        MarkUpdated();
        return consumed;
    }
}
