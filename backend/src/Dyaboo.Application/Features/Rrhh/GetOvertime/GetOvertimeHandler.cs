using Dyaboo.Application.Interfaces;
using Dyaboo.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Dyaboo.Application.Features.Rrhh.GetOvertime;

public class GetOvertimeHandler(IApplicationDbContext db)
    : IRequestHandler<GetOvertimeQuery, IReadOnlyList<OvertimeResult>>
{
    public async Task<IReadOnlyList<OvertimeResult>> Handle(
        GetOvertimeQuery request,
        CancellationToken cancellationToken)
    {
        var query = db.OvertimeEntries
            .Include(o => o.Employee)
            .AsNoTracking();

        if (request.Year is int year)
        {
            if (request.Month is int month)
            {
                var start = new DateOnly(year, month, 1);
                var end   = start.AddMonths(1);
                query = query.Where(o => o.Date >= start && o.Date < end);
            }
            else
            {
                query = query.Where(o => o.Date.Year == year);
            }
        }

        if (request.EmployeeId is Guid employeeId)
            query = query.Where(o => o.EmployeeId == employeeId);

        var entries = await query
            .OrderByDescending(o => o.Date)
            .ThenBy(o => o.Employee.FullName)
            .ToListAsync(cancellationToken);

        return entries.Select(MapToResult).ToList();
    }

    internal static OvertimeResult MapToResult(OvertimeEntry o) => new(
        o.Id,
        o.EmployeeId,
        o.Employee.FullName,
        o.Date,
        o.Type.ToString(),
        o.Hours,
        o.HourlyRateSnapshot,
        o.SurchargePercent,
        o.Amount,
        o.Notes);
}
