using Dyaboo.Application.Features.Rrhh.GetEmployees;
using Dyaboo.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Dyaboo.Application.Features.Rrhh.DeactivateEmployee;

public class DeactivateEmployeeHandler(IApplicationDbContext db)
    : IRequestHandler<DeactivateEmployeeCommand, EmployeeResult>
{
    public async Task<EmployeeResult> Handle(
        DeactivateEmployeeCommand request,
        CancellationToken cancellationToken)
    {
        var employee = await db.Employees
            .FirstOrDefaultAsync(e => e.Id == request.EmployeeId, cancellationToken)
            ?? throw new KeyNotFoundException($"Empleado '{request.EmployeeId}' no encontrado.");

        employee.Deactivate();
        await db.SaveChangesAsync(cancellationToken);

        return GetEmployeesHandler.MapToResult(employee);
    }
}
