using Dyaboo.Domain.Enums;
using Dyaboo.Domain.ValueObjects;

namespace Dyaboo.Domain.Entities;

public class ProductReference : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string ReferenceCode { get; private set; } = string.Empty;  // ej: DYB-2024-001
    public string Description { get; private set; } = string.Empty;
    public ProductCategory Category { get; private set; }
    public bool IsActive { get; private set; } = true;

    private readonly List<ProductVariant> _variants = [];
    public IReadOnlyCollection<ProductVariant> Variants => _variants.AsReadOnly();

    private ProductReference() { }  // requerido por EF Core

    public static ProductReference Create(
        string name,
        string referenceCode,
        ProductCategory category,
        string description = "")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(referenceCode);

        return new ProductReference
        {
            Name = name.Trim(),
            ReferenceCode = referenceCode.Trim().ToUpper(),
            Category = category,
            Description = description.Trim()
        };
    }

    public ProductVariant AddVariant(Size size, Color color, string sku, decimal costPrice)
    {
        var alreadyExists = _variants.Any(v =>
            v.Size.Code == size.Code && v.Color.HexCode == color.HexCode);

        if (alreadyExists)
            throw new InvalidOperationException(
                $"Ya existe una variante con talla '{size}' y color '{color}' para esta referencia.");

        var variant = ProductVariant.Create(Id, size, color, sku, costPrice);
        _variants.Add(variant);
        MarkUpdated();
        return variant;
    }

    public void Deactivate()
    {
        IsActive = false;
        MarkUpdated();
    }
}
