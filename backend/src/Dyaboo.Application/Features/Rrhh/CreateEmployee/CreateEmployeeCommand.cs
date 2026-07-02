using Dyaboo.Application.Features.Rrhh.GetEmployees;
using Dyaboo.Domain.Enums;
using MediatR;

namespace Dyaboo.Application.Features.Rrhh.CreateEmployee;

public record CreateEmployeeCommand(
    string FullName,
    string DocumentNumber,
    string JobTitle,
    CompanyArea Area,
    DateOnly HireDate,
    decimal MonthlySalary,
    int WeeklyHours) : IRequest<EmployeeResult>;
