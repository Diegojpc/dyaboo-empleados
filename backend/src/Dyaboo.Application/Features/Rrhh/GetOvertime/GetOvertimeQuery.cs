using MediatR;

namespace Dyaboo.Application.Features.Rrhh.GetOvertime;

public record GetOvertimeQuery(
    int? Year = null,
    int? Month = null,
    Guid? EmployeeId = null) : IRequest<IReadOnlyList<OvertimeResult>>;

public record OvertimeResult(
    Guid Id,
    Guid EmployeeId,
    string EmployeeName,
    DateOnly Date,
    string Type,
    decimal Hours,
    decimal HourlyRateSnapshot,
    decimal SurchargePercent,
    decimal Amount,
    string? Notes);
