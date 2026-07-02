using Dyaboo.Application.Features.Distribucion.GetCustomers;
using Dyaboo.Application.Interfaces;
using Dyaboo.Domain.Entities;
using MediatR;

namespace Dyaboo.Application.Features.Distribucion.CreateCustomer;

public class CreateCustomerHandler(IApplicationDbContext db)
    : IRequestHandler<CreateCustomerCommand, CustomerResult>
{
    public async Task<CustomerResult> Handle(
        CreateCustomerCommand request,
        CancellationToken cancellationToken)
    {
        var customer = Customer.Create(
            request.Name, request.Type, request.ContactName, request.Phone, request.City);

        await db.Customers.AddAsync(customer, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        return new CustomerResult(
            customer.Id, customer.Name, customer.Type.ToString(),
            customer.ContactName, customer.Phone, customer.City, customer.IsActive);
    }
}
