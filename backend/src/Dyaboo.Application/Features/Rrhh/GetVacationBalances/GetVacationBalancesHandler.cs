using Dyaboo.Application.Interfaces;
using Dyaboo.Domain.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Dyaboo.Application.Features.Rrhh.GetVacationBalances;

public class GetVacationBalancesHandler(IApplicationDbContext db)
    : IRequestHandler<GetVacationBalancesQuery, IReadOnlyList<VacationBalanceResult>>
{
    public async Task<IReadOnlyList<VacationBalanceResult>> Handle(
        GetVacationBalancesQuery request,
        CancellationToken cancellationToken)
    {
        var employees = await db.Employees
            .AsNoTracking()
            .Where(e => e.IsActive)
            .OrderBy(e => e.FullName)
            .ToListAsync(cancellationToken);

        var periods = await db.VacationPeriods
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var periodsByEmployee = periods
            .GroupBy(p => p.EmployeeId)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(p => p.StartDate).ToList());

        var today = DateOnly.FromDateTime(DateTime.Today);

        return employees.Select(e =>
        {
            var employeePeriods = periodsByEmployee.TryGetValue(e.Id, out var list)
                ? list
                : [];

            var accrued = VacationCalculator.AccruedDays(e.HireDate, today);
            var taken   = employeePeriods.Sum(p => p.BusinessDays);

            return new VacationBalanceResult(
                e.Id,
                e.FullName,
                e.JobTitle,
                e.HireDate,
                accrued,
                taken,
                Math.Round(accrued - taken, 2),
                employeePeriods.Select(p => new VacationPeriodResult(
                    p.Id, p.StartDate, p.EndDate, p.BusinessDays, p.Notes)).ToList());
        }).ToList();
    }
}
