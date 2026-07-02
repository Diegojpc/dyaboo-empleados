using Dyaboo.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Dyaboo.Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<ProductReference> ProductReferences { get; }
    DbSet<ProductVariant> ProductVariants { get; }
    DbSet<ProductImage> ProductImages { get; }
    DbSet<ProductionOrder> ProductionOrders { get; }
    DbSet<ProductionOrderItem> ProductionOrderItems { get; }
    DbSet<WarehouseLocation> WarehouseLocations { get; }
    DbSet<StockAssignment> StockAssignments { get; }
    DbSet<Confeccionista> Confeccionistas { get; }
    DbSet<CuttingOrder> CuttingOrders { get; }
    DbSet<CuttingOrderItem> CuttingOrderItems { get; }
    DbSet<SewingOrder> SewingOrders { get; }
    DbSet<SewingOrderItem> SewingOrderItems { get; }
    DbSet<Customer> Customers { get; }
    DbSet<SalesOrder> SalesOrders { get; }
    DbSet<SalesOrderItem> SalesOrderItems { get; }
    DbSet<Employee> Employees { get; }
    DbSet<OvertimeEntry> OvertimeEntries { get; }
    DbSet<VacationPeriod> VacationPeriods { get; }
    DbSet<Holiday> Holidays { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
