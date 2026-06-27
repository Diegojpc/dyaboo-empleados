using Dyaboo.Domain.Entities;
using Dyaboo.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dyaboo.Infrastructure.Persistence.Configurations;

public class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
{
    public void Configure(EntityTypeBuilder<ProductVariant> builder)
    {
        builder.ToTable("product_variants");

        builder.HasKey(v => v.Id);
        builder.Property(v => v.Id).ValueGeneratedNever();

        // Size — Owned Entity (columna: size_code)
        builder.OwnsOne(v => v.Size, size =>
        {
            size.Property(s => s.Code)
                .HasColumnName("size_code")
                .IsRequired()
                .HasMaxLength(10);
        });

        // Color — Owned Entity (columnas: color_name, color_hex)
        builder.OwnsOne(v => v.Color, color =>
        {
            color.Property(c => c.Name)
                .HasColumnName("color_name")
                .IsRequired()
                .HasMaxLength(100);
            color.Property(c => c.HexCode)
                .HasColumnName("color_hex")
                .IsRequired()
                .HasMaxLength(7);
        });

        builder.Property(v => v.SKU)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(v => v.SKU).IsUnique();

        builder.Property(v => v.StockQuantity).HasDefaultValue(0);
        builder.Property(v => v.CostPrice)
            .HasColumnType("numeric(18,4)")
            .IsRequired();
    }
}
