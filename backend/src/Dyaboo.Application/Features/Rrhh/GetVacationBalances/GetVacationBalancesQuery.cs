using MediatR;

namespace Dyaboo.Application.Features.Rrhh.GetVacationBalances;

public record GetVacationBalancesQuery : IRequest<IReadOnlyList<VacationBalanceResult>>;

public record VacationBalanceResult(
    Guid EmployeeId,
    string FullName,
    string JobTitle,
    DateOnly HireDate,
    decimal AccruedDays,
    int TakenDays,
    decimal BalanceDays,
    IReadOnlyList<VacationPeriodResult> Periods);

public record VacationPeriodResult(
    Guid Id,
    DateOnly StartDate,
    DateOnly EndDate,
    int BusinessDays,
    string? Notes);
