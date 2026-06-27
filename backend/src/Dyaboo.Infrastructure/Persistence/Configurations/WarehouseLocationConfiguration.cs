using Dyaboo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dyaboo.Infrastructure.Persistence.Configurations;

public class WarehouseLocationConfiguration : IEntityTypeConfiguration<WarehouseLocation>
{
    public void Configure(EntityTypeBuilder<WarehouseLocation> builder)
    {
        builder.ToTable("warehouse_locations");

        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id).ValueGeneratedNever();

        // WarehouseLocationCode — Owned Entity (3 columnas)
        builder.OwnsOne(l => l.LocationCode, lc =>
        {
            lc.Property(c => c.Aisle).HasColumnName("aisle").IsRequired().HasMaxLength(3);
            lc.Property(c => c.Shelf).HasColumnName("shelf").IsRequired();
            lc.Property(c => c.Level).HasColumnName("level").IsRequired();
            lc.Property(c => c.Code).HasColumnName("location_code").IsRequired().HasMaxLength(10);
            lc.HasIndex(c => c.Code).IsUnique();
        });

        builder.Property(l => l.Capacity).IsRequired();
        builder.Property(l => l.CurrentStock).HasDefaultValue(0).IsRequired();
        builder.Property(l => l.IsActive).HasDefaultValue(true).IsRequired();

        // Propiedades calculadas: ignoradas en BD
        builder.Ignore(l => l.AvailableSpace);
        builder.Ignore(l => l.IsFull);
        builder.Ignore(l => l.OccupancyPct);
    }
}
