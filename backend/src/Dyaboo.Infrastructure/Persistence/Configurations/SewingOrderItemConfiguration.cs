using Dyaboo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dyaboo.Infrastructure.Persistence.Configurations;

public class SewingOrderItemConfiguration : IEntityTypeConfiguration<SewingOrderItem>
{
    public void Configure(EntityTypeBuilder<SewingOrderItem> builder)
    {
        builder.ToTable("sewing_order_items");

        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).ValueGeneratedNever();

        builder.Property(i => i.QuantitySent).IsRequired();
        builder.Property(i => i.QuantityApproved).IsRequired();
        builder.Property(i => i.QuantityRejected).IsRequired();

        builder.HasOne(i => i.ProductVariant)
            .WithMany()
            .HasForeignKey(i => i.ProductVariantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(i => i.SewingOrderId);
    }
}
