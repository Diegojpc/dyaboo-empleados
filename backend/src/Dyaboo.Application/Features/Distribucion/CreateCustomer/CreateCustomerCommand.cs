using Dyaboo.Application.Features.Distribucion.GetCustomers;
using Dyaboo.Domain.Enums;
using MediatR;

namespace Dyaboo.Application.Features.Distribucion.CreateCustomer;

public record CreateCustomerCommand(
    string Name,
    CustomerType Type,
    string ContactName,
    string Phone,
    string City) : IRequest<CustomerResult>;
