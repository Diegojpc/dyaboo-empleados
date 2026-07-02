using Dyaboo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dyaboo.Infrastructure.Persistence.Configurations;

public class OvertimeEntryConfiguration : IEntityTypeConfiguration<OvertimeEntry>
{
    public void Configure(EntityTypeBuilder<OvertimeEntry> builder)
    {
        builder.ToTable("overtime_entries");

        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id).ValueGeneratedNever();

        builder.Property(o => o.Type).IsRequired().HasConversion<string>().HasMaxLength(50);
        builder.Property(o => o.Hours).IsRequired().HasColumnType("numeric(5,2)");
        builder.Property(o => o.HourlyRateSnapshot).IsRequired().HasColumnType("numeric(12,2)");
        builder.Property(o => o.SurchargePercent).IsRequired().HasColumnType("numeric(5,4)");
        builder.Property(o => o.Amount).IsRequired().HasColumnType("numeric(12,2)");
        builder.Property(o => o.Notes).HasMaxLength(500);

        builder.HasOne(o => o.Employee)
            .WithMany()
            .HasForeignKey(o => o.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(o => new { o.EmployeeId, o.Date });
    }
}
