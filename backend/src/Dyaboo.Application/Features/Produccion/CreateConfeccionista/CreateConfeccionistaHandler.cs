using Dyaboo.Application.Features.Produccion.GetConfeccionistas;
using Dyaboo.Application.Interfaces;
using Dyaboo.Domain.Entities;
using MediatR;

namespace Dyaboo.Application.Features.Produccion.CreateConfeccionista;

public class CreateConfeccionistaHandler(IApplicationDbContext db)
    : IRequestHandler<CreateConfeccionistaCommand, ConfeccionistaResult>
{
    public async Task<ConfeccionistaResult> Handle(
        CreateConfeccionistaCommand request,
        CancellationToken cancellationToken)
    {
        var confeccionista = Confeccionista.Create(
            request.Name, request.ContactName, request.Phone, request.City);

        await db.Confeccionistas.AddAsync(confeccionista, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        return new ConfeccionistaResult(
            confeccionista.Id, confeccionista.Name, confeccionista.ContactName,
            confeccionista.Phone, confeccionista.City, confeccionista.IsActive);
    }
}
