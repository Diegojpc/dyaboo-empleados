using Dyaboo.Application.Interfaces;
using Dyaboo.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Dyaboo.Infrastructure.Persistence;

public class DyabooDbContext(DbContextOptions<DyabooDbContext> options)
    : DbContext(options), IApplicationDbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<ProductReference> ProductReferences => Set<ProductReference>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<ProductionOrder> ProductionOrders => Set<ProductionOrder>();
    public DbSet<ProductionOrderItem> ProductionOrderItems => Set<ProductionOrderItem>();
    public DbSet<WarehouseLocation> WarehouseLocations => Set<WarehouseLocation>();
    public DbSet<StockAssignment> StockAssignments => Set<StockAssignment>();
    public DbSet<Confeccionista> Confeccionistas => Set<Confeccionista>();
    public DbSet<CuttingOrder> CuttingOrders => Set<CuttingOrder>();
    public DbSet<CuttingOrderItem> CuttingOrderItems => Set<CuttingOrderItem>();
    public DbSet<SewingOrder> SewingOrders => Set<SewingOrder>();
    public DbSet<SewingOrderItem> SewingOrderItems => Set<SewingOrderItem>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<SalesOrder> SalesOrders => Set<SalesOrder>();
    public DbSet<SalesOrderItem> SalesOrderItems => Set<SalesOrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DyabooDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
