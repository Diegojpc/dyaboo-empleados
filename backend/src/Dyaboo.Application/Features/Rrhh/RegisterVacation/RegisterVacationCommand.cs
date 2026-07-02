using Dyaboo.Application.Features.Rrhh.GetVacationBalances;
using MediatR;

namespace Dyaboo.Application.Features.Rrhh.RegisterVacation;

public record RegisterVacationCommand(
    Guid EmployeeId,
    DateOnly StartDate,
    DateOnly EndDate,
    string? Notes) : IRequest<VacationPeriodResult>;
