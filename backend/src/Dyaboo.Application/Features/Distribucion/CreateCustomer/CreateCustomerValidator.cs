using FluentValidation;

namespace Dyaboo.Application.Features.Distribucion.CreateCustomer;

public class CreateCustomerValidator : AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre del cliente es requerido.")
            .MaximumLength(150);

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("El tipo de cliente no es válido.");

        RuleFor(x => x.ContactName).MaximumLength(150);
        RuleFor(x => x.Phone).MaximumLength(30);
        RuleFor(x => x.City).MaximumLength(100);
    }
}
