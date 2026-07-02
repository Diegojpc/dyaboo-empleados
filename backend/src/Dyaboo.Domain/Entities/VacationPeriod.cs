namespace Dyaboo.Domain.Entities;

/// <summary>
/// Periodo de vacaciones disfrutado. Los días hábiles se calculan y congelan al
/// registrar (excluyen domingos y festivos; el sábado cuenta como hábil porque
/// la sede trabaja de lunes a sábado).
/// </summary>
public class VacationPeriod : BaseEntity
{
    public Guid EmployeeId { get; private set; }
    public Employee Employee { get; private set; } = null!;
    public DateOnly StartDate { get; private set; }
    public DateOnly EndDate { get; private set; }
    public int BusinessDays { get; private set; }
    public string? Notes { get; private set; }

    private VacationPeriod() { }  // requerido por EF Core

    public static VacationPeriod Create(
        Employee employee,
        DateOnly startDate,
        DateOnly endDate,
        int businessDays,
        string? notes = null)
    {
        ArgumentNullException.ThrowIfNull(employee);

        if (endDate < startDate)
            throw new ArgumentException("La fecha final no puede ser anterior a la inicial.");
        if (businessDays <= 0)
            throw new ArgumentException("El periodo debe incluir al menos un día hábil.");

        return new VacationPeriod
        {
            EmployeeId   = employee.Id,
            StartDate    = startDate,
            EndDate      = endDate,
            BusinessDays = businessDays,
            Notes        = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim()
        };
    }
}
