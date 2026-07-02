using Dyaboo.Application.Features.Rrhh.GetEmployees;
using MediatR;

namespace Dyaboo.Application.Features.Rrhh.DeactivateEmployee;

public record DeactivateEmployeeCommand(Guid EmployeeId) : IRequest<EmployeeResult>;
