namespace Dyaboo.Domain.Entities;

public class SalesOrderItem : BaseEntity
{
    public Guid SalesOrderId { get; private set; }
    public Guid ProductVariantId { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }

    // Calculado (no mapeado en BD)
    public decimal LineTotal => Quantity * UnitPrice;

    // Navegación EF
    public SalesOrder SalesOrder { get; private set; } = null!;
    public ProductVariant ProductVariant { get; private set; } = null!;

    private SalesOrderItem() { }

    internal static SalesOrderItem Create(Guid salesOrderId, Guid productVariantId, int quantity, decimal unitPrice)
    {
        if (quantity <= 0)
            throw new ArgumentException("La cantidad debe ser mayor a cero.");
        if (unitPrice < 0)
            throw new ArgumentException("El precio unitario no puede ser negativo.");

        return new SalesOrderItem
        {
            SalesOrderId     = salesOrderId,
            ProductVariantId = productVariantId,
            Quantity         = quantity,
            UnitPrice        = unitPrice
        };
    }
}
