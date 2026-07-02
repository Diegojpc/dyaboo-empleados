using Dyaboo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dyaboo.Infrastructure.Persistence.Configurations;

public class SalesOrderItemConfiguration : IEntityTypeConfiguration<SalesOrderItem>
{
    public void Configure(EntityTypeBuilder<SalesOrderItem> builder)
    {
        builder.ToTable("sales_order_items");

        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).ValueGeneratedNever();

        builder.Property(i => i.Quantity).IsRequired();
        builder.Property(i => i.UnitPrice)
            .HasColumnType("numeric(12,2)")
            .IsRequired();

        builder.Ignore(i => i.LineTotal);

        builder.HasOne(i => i.ProductVariant)
            .WithMany()
            .HasForeignKey(i => i.ProductVariantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(i => i.SalesOrderId);
    }
}
