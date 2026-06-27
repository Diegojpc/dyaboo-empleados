using Dyaboo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dyaboo.Infrastructure.Persistence.Configurations;

public class ProductionOrderItemConfiguration : IEntityTypeConfiguration<ProductionOrderItem>
{
    public void Configure(EntityTypeBuilder<ProductionOrderItem> builder)
    {
        builder.ToTable("production_order_items");

        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).ValueGeneratedNever();

        builder.Property(i => i.Quantity).IsRequired();

        builder.Property(i => i.MaterialCostPerUnit)
            .HasColumnType("numeric(18,4)").IsRequired();

        builder.Property(i => i.LaborCostPerUnit)
            .HasColumnType("numeric(18,4)").IsRequired();

        builder.Property(i => i.OverheadCostPerUnit)
            .HasColumnType("numeric(18,4)").IsRequired();

        // Propiedades calculadas: ignoradas en BD
        builder.Ignore(i => i.TotalMaterialCost);
        builder.Ignore(i => i.TotalLaborCost);
        builder.Ignore(i => i.TotalOverheadCost);
        builder.Ignore(i => i.TotalLineCost);

        builder.HasOne(i => i.ProductVariant)
            .WithMany()
            .HasForeignKey(i => i.ProductVariantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
