using MediatR;

namespace Dyaboo.Application.Features.Rrhh.GetMonthlySummary;

/// <summary>
/// Resumen de novedades del mes por empleado: horas por tipo, total de recargos
/// y días de vacaciones. Formato plano pensado para exportar a CSV y llevarlo
/// a software contable (Siigo/Alegra).
/// </summary>
public record GetMonthlySummaryQuery(int Year, int Month) : IRequest<IReadOnlyList<MonthlySummaryRow>>;

public record MonthlySummaryRow(
    Guid EmployeeId,
    string FullName,
    string DocumentNumber,
    string JobTitle,
    decimal MonthlySalary,
    decimal HorasExtraDiurna,
    decimal HorasExtraNocturna,
    decimal HorasRecargoNocturno,
    decimal HorasDominicalFestivo,
    decimal HorasExtraDiurnaDominical,
    decimal HorasExtraNocturnaDominical,
    decimal TotalHoras,
    decimal TotalRecargos,
    int DiasVacaciones);
