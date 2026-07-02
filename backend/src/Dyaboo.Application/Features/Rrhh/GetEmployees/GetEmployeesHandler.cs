using Dyaboo.Application.Interfaces;
using Dyaboo.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Dyaboo.Application.Features.Rrhh.GetEmployees;

public class GetEmployeesHandler(IApplicationDbContext db)
    : IRequestHandler<GetEmployeesQuery, IReadOnlyList<EmployeeResult>>
{
    public async Task<IReadOnlyList<EmployeeResult>> Handle(
        GetEmployeesQuery request,
        CancellationToken cancellationToken)
    {
        var employees = await db.Employees
            .AsNoTracking()
            .OrderBy(e => e.FullName)
            .ToListAsync(cancellationToken);

        return employees.Select(MapToResult).ToList();
    }

    internal static EmployeeResult MapToResult(Employee e) => new(
        e.Id,
        e.FullName,
        e.DocumentNumber,
        e.JobTitle,
        e.Area.ToString(),
        e.Direction.ToString(),
        e.HireDate,
        e.MonthlySalary,
        e.WeeklyHours,
        Math.Round(e.HourlyRate, 2, MidpointRounding.AwayFromZero),
        e.IsActive);
}
