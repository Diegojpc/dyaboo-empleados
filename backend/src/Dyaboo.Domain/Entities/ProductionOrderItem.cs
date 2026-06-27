using Dyaboo.Domain.ValueObjects;

namespace Dyaboo.Domain.Entities;

public class ProductionOrderItem : BaseEntity
{
    public Guid ProductionOrderId { get; private set; }
    public Guid ProductVariantId { get; private set; }

    public int Quantity { get; private set; }

    // Costos unitarios capturados en el momento del cálculo
    public decimal MaterialCostPerUnit { get; private set; }
    public decimal LaborCostPerUnit { get; private set; }
    public decimal OverheadCostPerUnit { get; private set; }

    // Totales de línea
    public decimal TotalMaterialCost => Math.Round(MaterialCostPerUnit * Quantity, 4);
    public decimal TotalLaborCost    => Math.Round(LaborCostPerUnit    * Quantity, 4);
    public decimal TotalOverheadCost => Math.Round(OverheadCostPerUnit * Quantity, 4);
    public decimal TotalLineCost     => TotalMaterialCost + TotalLaborCost + TotalOverheadCost;

    // Navegación EF
    public ProductionOrder ProductionOrder { get; private set; } = null!;
    public ProductVariant ProductVariant { get; private set; } = null!;

    private ProductionOrderItem() { }

    internal static ProductionOrderItem Create(
        Guid productionOrderId,
        Guid productVariantId,
        int quantity,
        decimal materialCostPerUnit,
        decimal laborCostPerUnit,
        decimal overheadPercentage)
    {
        if (quantity <= 0) throw new ArgumentException("La cantidad debe ser mayor a cero.");
        if (materialCostPerUnit < 0) throw new ArgumentException("El costo de material no puede ser negativo.");
        if (laborCostPerUnit < 0) throw new ArgumentException("El costo de mano de obra no puede ser negativo.");

        var overheadPerUnit = Math.Round((materialCostPerUnit + laborCostPerUnit) * overheadPercentage / 100, 4);

        return new ProductionOrderItem
        {
            ProductionOrderId     = productionOrderId,
            ProductVariantId      = productVariantId,
            Quantity              = quantity,
            MaterialCostPerUnit   = materialCostPerUnit,
            LaborCostPerUnit      = laborCostPerUnit,
            OverheadCostPerUnit   = overheadPerUnit
        };
    }
}
