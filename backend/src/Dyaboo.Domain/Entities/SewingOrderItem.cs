namespace Dyaboo.Domain.Entities;

public class SewingOrderItem : BaseEntity
{
    public Guid SewingOrderId { get; private set; }
    public Guid ProductVariantId { get; private set; }
    public int QuantitySent { get; private set; }
    public int QuantityApproved { get; private set; }
    public int QuantityRejected { get; private set; }

    // Navegación EF
    public SewingOrder SewingOrder { get; private set; } = null!;
    public ProductVariant ProductVariant { get; private set; } = null!;

    private SewingOrderItem() { }

    internal static SewingOrderItem Create(Guid sewingOrderId, Guid productVariantId, int quantitySent)
    {
        if (quantitySent <= 0)
            throw new ArgumentException("La cantidad enviada debe ser mayor a cero.");

        return new SewingOrderItem
        {
            SewingOrderId    = sewingOrderId,
            ProductVariantId = productVariantId,
            QuantitySent     = quantitySent
        };
    }

    internal void RegisterReception(int approved, int rejected)
    {
        if (approved < 0 || rejected < 0)
            throw new ArgumentException("Las cantidades aprobadas y rechazadas no pueden ser negativas.");
        if (approved + rejected != QuantitySent)
            throw new InvalidOperationException(
                $"Aprobadas ({approved}) + rechazadas ({rejected}) debe igualar lo enviado ({QuantitySent}).");

        QuantityApproved = approved;
        QuantityRejected = rejected;
        MarkUpdated();
    }
}
