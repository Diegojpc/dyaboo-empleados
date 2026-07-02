using FluentValidation;

namespace Dyaboo.Application.Features.Produccion.CreateCuttingOrder;

public class CreateCuttingOrderValidator : AbstractValidator<CreateCuttingOrderCommand>
{
    public CreateCuttingOrderValidator()
    {
        RuleFor(x => x.ProductReferenceId).NotEmpty();

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("La orden de corte debe tener al menos un ítem.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.Quantity)
                .GreaterThan(0).WithMessage("La cantidad a cortar debe ser mayor a cero.");
        });

        RuleFor(x => x.Notes).MaximumLength(500);
    }
}
