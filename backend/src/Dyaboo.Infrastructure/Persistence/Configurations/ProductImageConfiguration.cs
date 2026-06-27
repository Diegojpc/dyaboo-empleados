using Dyaboo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dyaboo.Infrastructure.Persistence.Configurations;

public class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
{
    public void Configure(EntityTypeBuilder<ProductImage> builder)
    {
        builder.ToTable("product_images");

        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).ValueGeneratedNever();

        builder.Property(i => i.FileName).IsRequired().HasMaxLength(260);
        builder.Property(i => i.OriginalName).IsRequired().HasMaxLength(260);
        builder.Property(i => i.ContentType).IsRequired().HasMaxLength(100);
        builder.Property(i => i.FileSize).IsRequired();
        builder.Property(i => i.SortOrder).HasDefaultValue(0);

        builder.HasOne(i => i.ProductReference)
            .WithMany(p => p.Images)
            .HasForeignKey(i => i.ProductReferenceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
