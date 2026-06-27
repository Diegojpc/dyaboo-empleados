using Dyaboo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dyaboo.Infrastructure.Persistence.Configurations;

public class ProductReferenceConfiguration : IEntityTypeConfiguration<ProductReference>
{
    public void Configure(EntityTypeBuilder<ProductReference> builder)
    {
        builder.ToTable("product_references");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.ReferenceCode)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(p => p.ReferenceCode).IsUnique();

        builder.Property(p => p.Description).HasMaxLength(1000);
        builder.Property(p => p.Category).IsRequired();
        builder.Property(p => p.IsActive).HasDefaultValue(true);

        builder.HasMany(p => p.Variants)
            .WithOne(v => v.ProductReference)
            .HasForeignKey(v => v.ProductReferenceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
