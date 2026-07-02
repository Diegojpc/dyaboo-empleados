using FluentValidation;

namespace Dyaboo.Application.Features.Distribucion.CreateSalesOrder;

public class CreateSalesOrderValidator : AbstractValidator<CreateSalesOrderCommand>
{
    public CreateSalesOrderValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("El pedido debe tener al menos un ítem.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.Quantity)
                .GreaterThan(0).WithMessage("La cantidad debe ser mayor a cero.");

            item.RuleFor(i => i.UnitPrice)
                .GreaterThanOrEqualTo(0).WithMessage("El precio unitario no puede ser negativo.");
        });

        RuleFor(x => x.Notes).MaximumLength(500);
    }
}
