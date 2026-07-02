using Dyaboo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dyaboo.Infrastructure.Persistence.Configurations;

public class StockAssignmentConfiguration : IEntityTypeConfiguration<StockAssignment>
{
    public void Configure(EntityTypeBuilder<StockAssignment> builder)
    {
        builder.ToTable("stock_assignments");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedNever();
        builder.Property(a => a.Quantity).IsRequired();
        builder.Property(a => a.RemainingQuantity).IsRequired();

        builder.HasOne(a => a.WarehouseLocation)
            .WithMany()
            .HasForeignKey(a => a.WarehouseLocationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.ProductVariant)
            .WithMany()
            .HasForeignKey(a => a.ProductVariantId)
            .OnDelete(DeleteBehavior.Restrict);

        // Índice compuesto para queries de status por ubicación
        builder.HasIndex(a => a.WarehouseLocationId);
        builder.HasIndex(a => a.ProductVariantId);
    }
}
