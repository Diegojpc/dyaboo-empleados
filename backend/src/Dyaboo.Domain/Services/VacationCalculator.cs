namespace Dyaboo.Domain.Services;

/// <summary>
/// Cálculo de días hábiles y causación de vacaciones (RIT Cap. XIII):
/// 15 días hábiles por año de servicio, prorrateados sobre año comercial de 360 días.
/// </summary>
public static class VacationCalculator
{
    private const decimal DaysPerYear = 15m;

    /// <summary>
    /// Días hábiles del rango [start, end]: excluye domingos y festivos.
    /// El sábado cuenta como hábil (la sede trabaja de lunes a sábado).
    /// </summary>
    public static int CountBusinessDays(DateOnly start, DateOnly end, IReadOnlySet<DateOnly> holidays)
    {
        if (end < start)
            throw new ArgumentException("La fecha final no puede ser anterior a la inicial.");

        var count = 0;
        for (var day = start; day <= end; day = day.AddDays(1))
            if (day.DayOfWeek != DayOfWeek.Sunday && !holidays.Contains(day))
                count++;

        return count;
    }

    /// <summary>
    /// Días de vacaciones causados a la fecha: díasTrabajados × 15 / 360
    /// (año comercial, prorrateo estándar de liquidación), 2 decimales.
    /// </summary>
    public static decimal AccruedDays(DateOnly hireDate, DateOnly asOf)
    {
        if (asOf <= hireDate) return 0m;

        var workedDays = asOf.DayNumber - hireDate.DayNumber;
        return Math.Round(workedDays * DaysPerYear / 360m, 2, MidpointRounding.AwayFromZero);
    }
}
