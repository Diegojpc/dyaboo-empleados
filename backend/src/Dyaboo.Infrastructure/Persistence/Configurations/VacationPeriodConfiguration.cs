using Dyaboo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dyaboo.Infrastructure.Persistence.Configurations;

public class VacationPeriodConfiguration : IEntityTypeConfiguration<VacationPeriod>
{
    public void Configure(EntityTypeBuilder<VacationPeriod> builder)
    {
        builder.ToTable("vacation_periods");

        builder.HasKey(v => v.Id);
        builder.Property(v => v.Id).ValueGeneratedNever();

        builder.Property(v => v.BusinessDays).IsRequired();
        builder.Property(v => v.Notes).HasMaxLength(500);

        builder.HasOne(v => v.Employee)
            .WithMany()
            .HasForeignKey(v => v.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(v => new { v.EmployeeId, v.StartDate });
    }
}
