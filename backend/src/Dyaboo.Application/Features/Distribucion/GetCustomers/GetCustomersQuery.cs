using MediatR;

namespace Dyaboo.Application.Features.Distribucion.GetCustomers;

public record GetCustomersQuery : IRequest<IReadOnlyList<CustomerResult>>;

public record CustomerResult(
    Guid Id,
    string Name,
    string Type,
    string ContactName,
    string Phone,
    string City,
    bool IsActive);
