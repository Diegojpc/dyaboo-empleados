using Dyaboo.Application.Features.Rrhh.GetEmployees;
using Dyaboo.Application.Interfaces;
using Dyaboo.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Dyaboo.Application.Features.Rrhh.CreateEmployee;

public class CreateEmployeeHandler(IApplicationDbContext db)
    : IRequestHandler<CreateEmployeeCommand, EmployeeResult>
{
    public async Task<EmployeeResult> Handle(
        CreateEmployeeCommand request,
        CancellationToken cancellationToken)
    {
        var document = request.DocumentNumber.Trim();
        var exists = await db.Employees
            .AnyAsync(e => e.DocumentNumber == document, cancellationToken);

        if (exists)
            throw new InvalidOperationException(
                $"Ya existe un empleado con la cédula '{document}'.");

        var employee = Employee.Create(
            request.FullName,
            request.DocumentNumber,
            request.JobTitle,
            request.Area,
            request.HireDate,
            request.MonthlySalary,
            request.WeeklyHours);

        db.Employees.Add(employee);
        await db.SaveChangesAsync(cancellationToken);

        return GetEmployeesHandler.MapToResult(employee);
    }
}
