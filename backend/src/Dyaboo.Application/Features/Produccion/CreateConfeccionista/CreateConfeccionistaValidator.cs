using FluentValidation;

namespace Dyaboo.Application.Features.Produccion.CreateConfeccionista;

public class CreateConfeccionistaValidator : AbstractValidator<CreateConfeccionistaCommand>
{
    public CreateConfeccionistaValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre del taller es requerido.")
            .MaximumLength(150);

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("El teléfono es requerido.")
            .MaximumLength(30);

        RuleFor(x => x.ContactName).MaximumLength(150);
        RuleFor(x => x.City).MaximumLength(100);
    }
}
