using Dyaboo.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Dyaboo.Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<ProductReference> ProductReferences { get; }
    DbSet<ProductVariant> ProductVariants { get; }
    DbSet<ProductionOrder> ProductionOrders { get; }
    DbSet<ProductionOrderItem> ProductionOrderItems { get; }
    DbSet<WarehouseLocation> WarehouseLocations { get; }
    DbSet<StockAssignment> StockAssignments { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
