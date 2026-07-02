using Dyaboo.Application.Interfaces;
using Dyaboo.Domain.Enums;
using Dyaboo.Domain.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Dyaboo.Application.Features.Rrhh.GetMonthlySummary;

public class GetMonthlySummaryHandler(IApplicationDbContext db)
    : IRequestHandler<GetMonthlySummaryQuery, IReadOnlyList<MonthlySummaryRow>>
{
    public async Task<IReadOnlyList<MonthlySummaryRow>> Handle(
        GetMonthlySummaryQuery request,
        CancellationToken cancellationToken)
    {
        if (request.Month is < 1 or > 12)
            throw new ArgumentException("El mes debe estar entre 1 y 12.");

        var start = new DateOnly(request.Year, request.Month, 1);
        var end   = start.AddMonths(1);

        var employees = await db.Employees
            .AsNoTracking()
            .OrderBy(e => e.FullName)
            .ToListAsync(cancellationToken);

        var entries = await db.OvertimeEntries
            .AsNoTracking()
            .Where(o => o.Date >= start && o.Date < end)
            .ToListAsync(cancellationToken);

        // Periodos que tocan el mes (pueden empezar antes o terminar después)
        var vacations = await db.VacationPeriods
            .AsNoTracking()
            .Where(p => p.StartDate < end && p.EndDate >= start)
            .ToListAsync(cancellationToken);

        var holidays = (await db.Holidays
            .AsNoTracking()
            .Where(h => h.Date >= start && h.Date < end)
            .Select(h => h.Date)
            .ToListAsync(cancellationToken)).ToHashSet();

        var entriesByEmployee   = entries.GroupBy(o => o.EmployeeId).ToDictionary(g => g.Key, g => g.ToList());
        var vacationsByEmployee = vacations.GroupBy(p => p.EmployeeId).ToDictionary(g => g.Key, g => g.ToList());

        var rows = new List<MonthlySummaryRow>();

        foreach (var e in employees)
        {
            var empEntries = entriesByEmployee.TryGetValue(e.Id, out var list) ? list : [];

            decimal HoursOf(OvertimeType t) => empEntries.Where(o => o.Type == t).Sum(o => o.Hours);

            // Días de vacaciones que caen dentro del mes (recorta periodos que lo cruzan)
            var vacationDays = 0;
            if (vacationsByEmployee.TryGetValue(e.Id, out var empVacations))
            {
                var lastDayOfMonth = end.AddDays(-1);
                foreach (var p in empVacations)
                {
                    var from = p.StartDate > start ? p.StartDate : start;
                    var to   = p.EndDate < lastDayOfMonth ? p.EndDate : lastDayOfMonth;
                    vacationDays += VacationCalculator.CountBusinessDays(from, to, holidays);
                }
            }

            if (empEntries.Count == 0 && vacationDays == 0) continue;

            rows.Add(new MonthlySummaryRow(
                e.Id,
                e.FullName,
                e.DocumentNumber,
                e.JobTitle,
                e.MonthlySalary,
                HoursOf(OvertimeType.ExtraDiurna),
                HoursOf(OvertimeType.ExtraNocturna),
                HoursOf(OvertimeType.RecargoNocturno),
                HoursOf(OvertimeType.DominicalFestivo),
                HoursOf(OvertimeType.ExtraDiurnaDominical),
                HoursOf(OvertimeType.ExtraNocturnaDominical),
                empEntries.Sum(o => o.Hours),
                empEntries.Sum(o => o.Amount),
                vacationDays));
        }

        return rows;
    }
}
