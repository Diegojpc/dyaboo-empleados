using MediatR;

namespace Dyaboo.Application.Features.Rrhh.GetEmployees;

public record GetEmployeesQuery : IRequest<IReadOnlyList<EmployeeResult>>;

public record EmployeeResult(
    Guid Id,
    string FullName,
    string DocumentNumber,
    string JobTitle,
    string Area,
    string Direction,
    DateOnly HireDate,
    decimal MonthlySalary,
    int WeeklyHours,
    decimal HourlyRate,
    bool IsActive);
