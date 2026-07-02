using Dyaboo.Domain.Entities;
using Dyaboo.Domain.Enums;
using Dyaboo.Domain.Services;

namespace Dyaboo.Domain.Tests;

public class VacationTests
{
    private static Employee Empleado(DateOnly? ingreso = null)
        => Employee.Create("Luisa Ríos", "1040506070", "Diseñadora", CompanyArea.Diseno,
            ingreso ?? new DateOnly(2024, 1, 1), 2_500_000m, 44);

    [Fact]
    public void CountBusinessDays_ExcluyeDomingosYFestivos_SabadoCuenta()
    {
        // 8-jun-2026 (lun, festivo Corpus Christi) → 20-jun-2026 (sáb)
        // Excluye: festivos 8 y 15-jun (Sagrado Corazón) y domingo 14-jun.
        // Cuentan: 9-13 jun (5, incluye sábado 13) + 16-20 jun (5, incluye sábado 20) − 1...
        // Total días 13 − 2 festivos − 1 domingo = 10
        var festivos = new HashSet<DateOnly>
        {
            new(2026, 6, 8),   // Corpus Christi
            new(2026, 6, 15),  // Sagrado Corazón
        };

        var dias = VacationCalculator.CountBusinessDays(
            new DateOnly(2026, 6, 8), new DateOnly(2026, 6, 20), festivos);

        Assert.Equal(10, dias);
    }

    [Fact]
    public void CountBusinessDays_RangoInvalidoLanza()
    {
        Assert.Throws<ArgumentException>(() => VacationCalculator.CountBusinessDays(
            new DateOnly(2026, 6, 20), new DateOnly(2026, 6, 8), new HashSet<DateOnly>()));
    }

    [Fact]
    public void AccruedDays_ProrrateaSobre360()
    {
        var ingreso = new DateOnly(2025, 1, 1);

        // 360 días después → 15,00 días causados
        Assert.Equal(15.00m, VacationCalculator.AccruedDays(ingreso, ingreso.AddDays(360)));
        // 180 días después → 7,50
        Assert.Equal(7.50m, VacationCalculator.AccruedDays(ingreso, ingreso.AddDays(180)));
        // Antes del ingreso → 0
        Assert.Equal(0m, VacationCalculator.AccruedDays(ingreso, ingreso.AddDays(-10)));
    }

    [Fact]
    public void VacationPeriod_ValidaRangoYDias()
    {
        var emp = Empleado();

        Assert.Throws<ArgumentException>(() => VacationPeriod.Create(
            emp, new DateOnly(2026, 6, 20), new DateOnly(2026, 6, 8), 5));
        Assert.Throws<ArgumentException>(() => VacationPeriod.Create(
            emp, new DateOnly(2026, 6, 8), new DateOnly(2026, 6, 20), 0));

        var periodo = VacationPeriod.Create(
            emp, new DateOnly(2026, 6, 8), new DateOnly(2026, 6, 20), 10, "vacaciones mitad de año");
        Assert.Equal(10, periodo.BusinessDays);
        Assert.Equal(emp.Id, periodo.EmployeeId);
    }
}
