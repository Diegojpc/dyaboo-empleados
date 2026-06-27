using Dyaboo.Domain.Enums;

namespace Dyaboo.Domain.Entities;

public class ProductionOrder : BaseEntity
{
    public string OrderCode { get; private set; } = string.Empty;
    public Guid ProductReferenceId { get; private set; }
    public ProductionOrderStatus Status { get; private set; } = ProductionOrderStatus.Draft;
    public decimal OverheadPercentage { get; private set; }

    private readonly List<ProductionOrderItem> _items = [];
    public IReadOnlyCollection<ProductionOrderItem> Items => _items.AsReadOnly();

    // Totales calculados sobre los items
    public decimal TotalMaterialCost => _items.Sum(i => i.TotalMaterialCost);
    public decimal TotalLaborCost    => _items.Sum(i => i.TotalLaborCost);
    public decimal TotalOverheadCost => _items.Sum(i => i.TotalOverheadCost);
    public decimal GrandTotal        => _items.Sum(i => i.TotalLineCost);
    public int     TotalUnits        => _items.Sum(i => i.Quantity);

    // Navegación EF
    public ProductReference ProductReference { get; private set; } = null!;

    private ProductionOrder() { }

    public static ProductionOrder Create(
        string orderCode,
        Guid productReferenceId,
        decimal overheadPercentage)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(orderCode);
        if (overheadPercentage < 0 || overheadPercentage > 100)
            throw new ArgumentException("El porcentaje de CIF debe estar entre 0 y 100.");

        return new ProductionOrder
        {
            OrderCode           = orderCode.Trim().ToUpper(),
            ProductReferenceId  = productReferenceId,
            OverheadPercentage  = overheadPercentage
        };
    }

    public ProductionOrderItem AddItem(
        Guid productVariantId,
        int quantity,
        decimal materialCostPerUnit,
        decimal laborCostPerUnit)
    {
        if (Status != ProductionOrderStatus.Draft)
            throw new InvalidOperationException("Solo se pueden agregar ítems a órdenes en estado Draft.");

        var item = ProductionOrderItem.Create(
            Id, productVariantId, quantity,
            materialCostPerUnit, laborCostPerUnit, OverheadPercentage);

        _items.Add(item);
        MarkUpdated();
        return item;
    }

    public void Approve()
    {
        if (!_items.Any())
            throw new InvalidOperationException("No se puede aprobar una orden sin ítems.");
        Status = ProductionOrderStatus.Approved;
        MarkUpdated();
    }
}
