using Dyaboo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dyaboo.Infrastructure.Persistence.Configurations;

public class SewingOrderConfiguration : IEntityTypeConfiguration<SewingOrder>
{
    public void Configure(EntityTypeBuilder<SewingOrder> builder)
    {
        builder.ToTable("sewing_orders");

        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id).ValueGeneratedNever();

        builder.Property(o => o.OrderCode).IsRequired().HasMaxLength(50);
        builder.HasIndex(o => o.OrderCode).IsUnique();

        builder.Property(o => o.Status).IsRequired();

        builder.Ignore(o => o.TotalSent);
        builder.Ignore(o => o.TotalApproved);
        builder.Ignore(o => o.TotalRejected);

        // 1:1 con la orden de corte en el POC
        builder.HasIndex(o => o.CuttingOrderId).IsUnique();

        builder.HasOne(o => o.CuttingOrder)
            .WithMany()
            .HasForeignKey(o => o.CuttingOrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.Confeccionista)
            .WithMany()
            .HasForeignKey(o => o.ConfeccionistaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(o => o.Items)
            .WithOne(i => i.SewingOrder)
            .HasForeignKey(i => i.SewingOrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
