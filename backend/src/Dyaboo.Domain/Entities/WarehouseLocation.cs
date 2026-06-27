using Dyaboo.Domain.ValueObjects;

namespace Dyaboo.Domain.Entities;

public class WarehouseLocation : BaseEntity
{
    public WarehouseLocationCode LocationCode { get; private set; } = null!;
    public int Capacity { get; private set; }
    public int CurrentStock { get; private set; }
    public bool IsActive { get; private set; } = true;

    public int AvailableSpace => Capacity - CurrentStock;
    public bool IsFull => CurrentStock >= Capacity;
    public double OccupancyPct => Capacity > 0 ? Math.Round((double)CurrentStock / Capacity * 100, 2) : 0;

    private WarehouseLocation() { }

    public static WarehouseLocation Create(string aisle, int shelf, int level, int capacity)
    {
        if (capacity <= 0) throw new ArgumentException("La capacidad debe ser mayor a cero.");
        return new WarehouseLocation
        {
            LocationCode = WarehouseLocationCode.From(aisle, shelf, level),
            Capacity = capacity
        };
    }

    public int Accommodate(int requestedUnits)
    {
        if (!IsActive) throw new InvalidOperationException($"La ubicación {LocationCode} está inactiva.");
        if (IsFull) return 0;

        var assigned = Math.Min(requestedUnits, AvailableSpace);
        CurrentStock += assigned;
        MarkUpdated();
        return assigned;
    }

    public void RemoveStock(int units)
    {
        if (units > CurrentStock)
            throw new InvalidOperationException($"Stock insuficiente en {LocationCode}.");
        CurrentStock -= units;
        MarkUpdated();
    }
}
