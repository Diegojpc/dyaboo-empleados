using Dyaboo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dyaboo.Infrastructure.Persistence.Configurations;

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("employees");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();

        builder.Property(e => e.FullName).IsRequired().HasMaxLength(150);
        builder.Property(e => e.DocumentNumber).IsRequired().HasMaxLength(20);
        builder.Property(e => e.JobTitle).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Area).IsRequired().HasConversion<string>().HasMaxLength(50);
        builder.Property(e => e.MonthlySalary).IsRequired().HasColumnType("numeric(12,2)");
        builder.Property(e => e.WeeklyHours).IsRequired();

        builder.HasIndex(e => e.DocumentNumber).IsUnique();

        builder.Ignore(e => e.Direction);
        builder.Ignore(e => e.HourlyRate);
    }
}
