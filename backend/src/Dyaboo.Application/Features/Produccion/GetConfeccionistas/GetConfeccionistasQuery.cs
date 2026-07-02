using MediatR;

namespace Dyaboo.Application.Features.Produccion.GetConfeccionistas;

public record GetConfeccionistasQuery : IRequest<IReadOnlyList<ConfeccionistaResult>>;

public record ConfeccionistaResult(
    Guid Id,
    string Name,
    string ContactName,
    string Phone,
    string City,
    bool IsActive);
