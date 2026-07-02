using Dyaboo.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Dyaboo.Application.Features.Produccion.GetConfeccionistas;

public class GetConfeccionistasHandler(IApplicationDbContext db)
    : IRequestHandler<GetConfeccionistasQuery, IReadOnlyList<ConfeccionistaResult>>
{
    public async Task<IReadOnlyList<ConfeccionistaResult>> Handle(
        GetConfeccionistasQuery request,
        CancellationToken cancellationToken)
    {
        return await db.Confeccionistas
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new ConfeccionistaResult(
                c.Id, c.Name, c.ContactName, c.Phone, c.City, c.IsActive))
            .ToListAsync(cancellationToken);
    }
}
