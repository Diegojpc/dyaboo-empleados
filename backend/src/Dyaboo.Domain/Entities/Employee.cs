using Dyaboo.Domain.Enums;

namespace Dyaboo.Domain.Entities;

/// <summary>
/// Empleado interno según el organigrama de Alianza BSH. No se vincula a User:
/// el módulo RRHH es administrado por Gestión Humana, sin autoservicio.
/// </summary>
public class Employee : BaseEntity
{
    public string FullName { get; private set; } = string.Empty;
    public string DocumentNumber { get; private set; } = string.Empty;
    public string JobTitle { get; private set; } = string.Empty;
    public CompanyArea Area { get; private set; }
    public DateOnly HireDate { get; private set; }
    public decimal MonthlySalary { get; private set; }
    public int WeeklyHours { get; private set; }
    public bool IsActive { get; private set; } = true;

    /// <summary>Dirección del organigrama a la que pertenece el área.</summary>
    public Directorate Direction => Area switch
    {
        CompanyArea.Diseno or CompanyArea.Mercadeo or CompanyArea.Comercial or CompanyArea.Tiendas
            => Directorate.Comercial,
        CompanyArea.Corte or CompanyArea.Produccion or CompanyArea.Logistica
            => Directorate.Operaciones,
        _ => Directorate.Administrativa
    };

    /// <summary>
    /// Valor de la hora ordinaria: salario / (jornada semanal × 5).
    /// Regla del divisor mensual = jornada semanal × 30/6 (44h→220, 42h→210).
    /// </summary>
    public decimal HourlyRate => MonthlySalary / (WeeklyHours * 5m);

    private Employee() { }  // requerido por EF Core

    public static Employee Create(
        string fullName,
        string documentNumber,
        string jobTitle,
        CompanyArea area,
        DateOnly hireDate,
        decimal monthlySalary,
        int weeklyHours)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fullName);
        ArgumentException.ThrowIfNullOrWhiteSpace(documentNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(jobTitle);

        if (monthlySalary <= 0)
            throw new ArgumentException("El salario mensual debe ser mayor a cero.");
        if (weeklyHours is < 1 or > 60)
            throw new ArgumentException("La jornada semanal debe estar entre 1 y 60 horas.");

        return new Employee
        {
            FullName       = fullName.Trim(),
            DocumentNumber = documentNumber.Trim(),
            JobTitle       = jobTitle.Trim(),
            Area           = area,
            HireDate       = hireDate,
            MonthlySalary  = monthlySalary,
            WeeklyHours    = weeklyHours
        };
    }

    public void UpdateCompensation(decimal monthlySalary, int weeklyHours)
    {
        if (monthlySalary <= 0)
            throw new ArgumentException("El salario mensual debe ser mayor a cero.");
        if (weeklyHours is < 1 or > 60)
            throw new ArgumentException("La jornada semanal debe estar entre 1 y 60 horas.");

        MonthlySalary = monthlySalary;
        WeeklyHours   = weeklyHours;
        MarkUpdated();
    }

    public void Deactivate()
    {
        IsActive = false;
        MarkUpdated();
    }
}
