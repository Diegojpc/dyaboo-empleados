using Dyaboo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dyaboo.Infrastructure.Persistence.Configurations;

public class CuttingOrderItemConfiguration : IEntityTypeConfiguration<CuttingOrderItem>
{
    public void Configure(EntityTypeBuilder<CuttingOrderItem> builder)
    {
        builder.ToTable("cutting_order_items");

        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).ValueGeneratedNever();

        builder.Property(i => i.PlannedQuantity).IsRequired();
        builder.Property(i => i.CutQuantity).IsRequired();

        builder.HasOne(i => i.ProductVariant)
            .WithMany()
            .HasForeignKey(i => i.ProductVariantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(i => i.CuttingOrderId);
    }
}
