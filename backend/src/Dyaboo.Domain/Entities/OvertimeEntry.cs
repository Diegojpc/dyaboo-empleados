using Dyaboo.Domain.Enums;
using Dyaboo.Domain.Services;

namespace Dyaboo.Domain.Entities;

/// <summary>
/// Novedad de horas de un empleado (registro directo, no reloj de entrada/salida).
/// El valor hora, el porcentaje y el monto se congelan al crear la novedad para
/// que cambios posteriores de salario no alteren el histórico.
/// </summary>
public class OvertimeEntry : BaseEntity
{
    /// <summary>Máximo legal de horas extra por día (RIT Art. 31).</summary>
    public const decimal MaxDailyExtraHours = 2m;

    public Guid EmployeeId { get; private set; }
    public Employee Employee { get; private set; } = null!;
    public DateOnly Date { get; private set; }
    public OvertimeType Type { get; private set; }
    public decimal Hours { get; private set; }
    public decimal HourlyRateSnapshot { get; private set; }
    public decimal SurchargePercent { get; private set; }
    public decimal Amount { get; private set; }
    public string? Notes { get; private set; }

    private OvertimeEntry() { }  // requerido por EF Core

    public static OvertimeEntry Create(
        Employee employee,
        DateOnly date,
        OvertimeType type,
        decimal hours,
        string? notes = null)
    {
        ArgumentNullException.ThrowIfNull(employee);

        if (hours <= 0)
            throw new ArgumentException("Las horas deben ser mayores a cero.");
        if (hours > 24)
            throw new ArgumentException("Las horas no pueden superar 24 en un día.");
        if (SurchargeCalculator.IsExtraHours(type) && hours > MaxDailyExtraHours)
            throw new ArgumentException(
                $"Las horas extra no pueden superar {MaxDailyExtraHours:0} por día (RIT Art. 31).");

        var rate    = employee.HourlyRate;
        var percent = SurchargeCalculator.GetSurchargePercent(type, date);

        return new OvertimeEntry
        {
            EmployeeId         = employee.Id,
            Date               = date,
            Type               = type,
            Hours              = hours,
            HourlyRateSnapshot = Math.Round(rate, 2, MidpointRounding.AwayFromZero),
            SurchargePercent   = percent,
            Amount             = SurchargeCalculator.CalculateAmount(type, date, hours, rate),
            Notes              = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim()
        };
    }
}
