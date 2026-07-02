using Dyaboo.Domain.Enums;

namespace Dyaboo.Domain.Services;

/// <summary>
/// Cálculo de recargos según el RIT de Alianza BSH (Arts. 33-34 y 44)
/// y la Ley 2466/2025 (recargo dominical/festivo progresivo por fecha).
/// </summary>
public static class SurchargeCalculator
{
    /// <summary>
    /// Recargo dominical/festivo vigente en la fecha de la novedad:
    /// 75% histórico, 80% desde 1-jul-2025, 90% desde 1-jul-2026, 100% desde 1-jul-2027.
    /// </summary>
    public static decimal GetSundayHolidayPercent(DateOnly date) => date switch
    {
        _ when date >= new DateOnly(2027, 7, 1) => 1.00m,
        _ when date >= new DateOnly(2026, 7, 1) => 0.90m,
        _ when date >= new DateOnly(2025, 7, 1) => 0.80m,
        _                                       => 0.75m
    };

    public static decimal GetSurchargePercent(OvertimeType type, DateOnly date) => type switch
    {
        OvertimeType.ExtraDiurna            => 0.25m,
        OvertimeType.ExtraNocturna          => 0.75m,
        OvertimeType.RecargoNocturno        => 0.35m,
        OvertimeType.DominicalFestivo       => GetSundayHolidayPercent(date),
        OvertimeType.ExtraDiurnaDominical   => 0.25m + GetSundayHolidayPercent(date),
        OvertimeType.ExtraNocturnaDominical => 0.75m + GetSundayHolidayPercent(date),
        _ => throw new ArgumentException($"Tipo de novedad desconocido: {type}")
    };

    /// <summary>
    /// Valor a pagar por la novedad. Los tipos Extra* pagan la hora completa más
    /// el recargo (horas × valorHora × (1 + %)); los tipos de solo recargo
    /// (RecargoNocturno, DominicalFestivo) pagan únicamente el recargo porque la
    /// hora base ya está incluida en el salario (horas × valorHora × %).
    /// </summary>
    public static decimal CalculateAmount(OvertimeType type, DateOnly date, decimal hours, decimal hourlyRate)
    {
        var percent = GetSurchargePercent(type, date);
        var factor  = IsExtraHours(type) ? 1m + percent : percent;
        return Math.Round(hours * hourlyRate * factor, 2, MidpointRounding.AwayFromZero);
    }

    /// <summary>Tipos que corresponden a horas trabajadas fuera de la jornada.</summary>
    public static bool IsExtraHours(OvertimeType type) => type is
        OvertimeType.ExtraDiurna or
        OvertimeType.ExtraNocturna or
        OvertimeType.ExtraDiurnaDominical or
        OvertimeType.ExtraNocturnaDominical;
}
