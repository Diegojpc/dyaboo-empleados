using Dyaboo.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Dyaboo.Application.Features.Rrhh.GetHolidays;

public class GetHolidaysHandler(IApplicationDbContext db)
    : IRequestHandler<GetHolidaysQuery, IReadOnlyList<HolidayResult>>
{
    public async Task<IReadOnlyList<HolidayResult>> Handle(
        GetHolidaysQuery request,
        CancellationToken cancellationToken)
    {
        var query = db.Holidays.AsNoTracking();

        if (request.Year is int year)
            query = query.Where(h => h.Date.Year == year);

        return await query
            .OrderBy(h => h.Date)
            .Select(h => new HolidayResult(h.Date, h.Name))
            .ToListAsync(cancellationToken);
    }
}
