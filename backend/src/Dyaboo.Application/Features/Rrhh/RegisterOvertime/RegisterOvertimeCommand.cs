using Dyaboo.Application.Features.Rrhh.GetOvertime;
using Dyaboo.Domain.Enums;
using MediatR;

namespace Dyaboo.Application.Features.Rrhh.RegisterOvertime;

public record RegisterOvertimeCommand(
    Guid EmployeeId,
    DateOnly Date,
    OvertimeType Type,
    decimal Hours,
    string? Notes) : IRequest<OvertimeResult>;
