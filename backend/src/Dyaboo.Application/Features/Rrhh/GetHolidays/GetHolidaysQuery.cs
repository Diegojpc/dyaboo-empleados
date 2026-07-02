using MediatR;

namespace Dyaboo.Application.Features.Rrhh.GetHolidays;

public record GetHolidaysQuery(int? Year = null) : IRequest<IReadOnlyList<HolidayResult>>;

public record HolidayResult(DateOnly Date, string Name);
