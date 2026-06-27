using Dyaboo.Domain.ValueObjects;

namespace Dyaboo.Domain.Entities;

public class ProductVariant : BaseEntity
{
    public Guid ProductReferenceId { get; private set; }

    // Value Objects almacenados como columnas propias (EF Owned Entity)
    public Size Size { get; private set; } = null!;
    public Color Color { get; private set; } = null!;

    public string SKU { get; private set; } = string.Empty;
    public int StockQuantity { get; private set; }
    public decimal CostPrice { get; private set; }

    // Navegación EF (sin setter público)
    public ProductReference ProductReference { get; private set; } = null!;

    private ProductVariant() { }  // requerido por EF Core

    internal static ProductVariant Create(
        Guid productReferenceId,
        Size size,
        Color color,
        string sku,
        decimal costPrice)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sku);
        if (costPrice < 0) throw new ArgumentException("El costo no puede ser negativo.");

        return new ProductVariant
        {
            ProductReferenceId = productReferenceId,
            Size = size,
            Color = color,
            SKU = sku.Trim().ToUpper(),
            CostPrice = costPrice,
            StockQuantity = 0
        };
    }

    public void AdjustStock(int delta)
    {
        if (StockQuantity + delta < 0)
            throw new InvalidOperationException($"Stock insuficiente para la variante {SKU}.");
        StockQuantity += delta;
        MarkUpdated();
    }
}
