using Dyaboo.Application.Features.Produccion.GetConfeccionistas;
using MediatR;

namespace Dyaboo.Application.Features.Produccion.CreateConfeccionista;

public record CreateConfeccionistaCommand(
    string Name,
    string ContactName,
    string Phone,
    string City) : IRequest<ConfeccionistaResult>;
