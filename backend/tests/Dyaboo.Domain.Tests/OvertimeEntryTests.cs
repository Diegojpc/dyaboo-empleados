using Dyaboo.Domain.Entities;
using Dyaboo.Domain.Enums;

namespace Dyaboo.Domain.Tests;

public class OvertimeEntryTests
{
    private static Employee Empleado(decimal salario = 1_800_000m)
        => Employee.Create("Ana Gómez", "1030405060", "Auxiliar de corte", CompanyArea.Corte,
            new DateOnly(2024, 3, 1), salario, 44);

    [Fact]
    public void Create_CongelaSnapshotYCalculaMonto()
    {
        var emp = Empleado(); // hora = 1.800.000/220 = 8.181,8181...
        var entry = OvertimeEntry.Create(emp, new DateOnly(2026, 7, 5), OvertimeType.ExtraDiurnaDominical, 2m);

        Assert.Equal(8_181.82m, entry.HourlyRateSnapshot);
        Assert.Equal(1.15m, entry.SurchargePercent); // 25% + 90% dominical
        Assert.Equal(35_181.82m, entry.Amount);      // 2 × 8.181,8181 × 2,15

        // Un aumento de salario posterior no altera la novedad ya registrada
        emp.UpdateCompensation(3_000_000m, 44);
        Assert.Equal(35_181.82m, entry.Amount);
        Assert.Equal(8_181.82m, entry.HourlyRateSnapshot);
    }

    [Fact]
    public void Create_RechazaMasDeDosHorasExtraPorDia()
    {
        var emp = Empleado();
        Assert.Throws<ArgumentException>(() =>
            OvertimeEntry.Create(emp, new DateOnly(2026, 7, 2), OvertimeType.ExtraDiurna, 3m));
    }

    [Fact]
    public void Create_PermiteMasDeDosHorasEnTiposDeSoloRecargo()
    {
        var emp = Empleado();
        var entry = OvertimeEntry.Create(emp, new DateOnly(2026, 7, 2), OvertimeType.RecargoNocturno, 8m);
        Assert.Equal(8m, entry.Hours);
    }

    [Fact]
    public void Create_RechazaHorasInvalidas()
    {
        var emp = Empleado();
        Assert.Throws<ArgumentException>(() =>
            OvertimeEntry.Create(emp, new DateOnly(2026, 7, 2), OvertimeType.RecargoNocturno, 0m));
        Assert.Throws<ArgumentException>(() =>
            OvertimeEntry.Create(emp, new DateOnly(2026, 7, 2), OvertimeType.RecargoNocturno, 25m));
    }
}
