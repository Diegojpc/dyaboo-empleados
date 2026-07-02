using Dyaboo.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Dyaboo.Application.Features.Distribucion.GetCustomers;

public class GetCustomersHandler(IApplicationDbContext db)
    : IRequestHandler<GetCustomersQuery, IReadOnlyList<CustomerResult>>
{
    public async Task<IReadOnlyList<CustomerResult>> Handle(
        GetCustomersQuery request,
        CancellationToken cancellationToken)
    {
        var customers = await db.Customers
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);

        return customers.Select(c => new CustomerResult(
            c.Id, c.Name, c.Type.ToString(), c.ContactName, c.Phone, c.City, c.IsActive))
            .ToList();
    }
}
