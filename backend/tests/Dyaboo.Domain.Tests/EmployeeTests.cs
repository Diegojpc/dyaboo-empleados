using Dyaboo.Domain.Entities;
using Dyaboo.Domain.Enums;

namespace Dyaboo.Domain.Tests;

public class EmployeeTests
{
    private static Employee Crear(decimal salario = 1_800_000m, int jornada = 44, CompanyArea area = CompanyArea.Corte)
        => Employee.Create("Juan Pérez", "1020304050", "Cortador", area, new DateOnly(2024, 1, 15), salario, jornada);

    [Fact]
    public void HourlyRate_UsaDivisorJornadaPorCinco()
    {
        // 44h → salario/220; 42h → salario/210
        Assert.Equal(1_800_000m / 220m, Crear(jornada: 44).HourlyRate);
        Assert.Equal(2_100_000m / 210m, Crear(salario: 2_100_000m, jornada: 42).HourlyRate);
    }

    [Theory]
    [InlineData(CompanyArea.Diseno,     Directorate.Comercial)]
    [InlineData(CompanyArea.Tiendas,    Directorate.Comercial)]
    [InlineData(CompanyArea.Corte,      Directorate.Operaciones)]
    [InlineData(CompanyArea.Logistica,  Directorate.Operaciones)]
    [InlineData(CompanyArea.Financiera, Directorate.Administrativa)]
    [InlineData(CompanyArea.Proyectos,  Directorate.Administrativa)]
    public void Direction_MapeaAreaADireccion(CompanyArea area, Directorate expected)
    {
        Assert.Equal(expected, Crear(area: area).Direction);
    }

    [Fact]
    public void Create_RechazaSalarioYJornadaInvalidos()
    {
        Assert.Throws<ArgumentException>(() => Crear(salario: 0));
        Assert.Throws<ArgumentException>(() => Crear(jornada: 0));
        Assert.Throws<ArgumentException>(() => Crear(jornada: 61));
        Assert.Throws<ArgumentException>(() =>
            Employee.Create("", "123", "Cargo", CompanyArea.Corte, new DateOnly(2024, 1, 1), 1_000_000m, 44));
    }

    [Fact]
    public void UpdateCompensation_ActualizaSalarioYJornada()
    {
        var emp = Crear();
        emp.UpdateCompensation(2_000_000m, 42);

        Assert.Equal(2_000_000m, emp.MonthlySalary);
        Assert.Equal(42, emp.WeeklyHours);
        Assert.Equal(2_000_000m / 210m, emp.HourlyRate);
    }
}
