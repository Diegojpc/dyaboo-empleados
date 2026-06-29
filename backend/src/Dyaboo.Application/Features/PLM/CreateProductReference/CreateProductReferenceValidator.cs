using FluentValidation;

namespace Dyaboo.Application.Features.PLM.CreateProductReference;

public class CreateProductReferenceValidator : AbstractValidator<CreateProductReferenceCommand>
{
    public CreateProductReferenceValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es requerido.")
            .MaximumLength(200).WithMessage("El nombre no puede superar 200 caracteres.");

        RuleFor(x => x.ReferenceCode)
            .NotEmpty().WithMessage("El código es requerido.")
            .MaximumLength(50).WithMessage("El código no puede superar 50 caracteres.")
            .Matches(@"^[A-Za-z0-9\-_]+$").WithMessage("El código solo puede contener letras, números, guiones y guiones bajos.");

        RuleFor(x => x.Variants)
            .NotEmpty().WithMessage("Debe tener al menos una variante.")
            .Must(v => v.Count <= 200).WithMessage("No se pueden crear más de 200 variantes por referencia.");

        RuleForEach(x => x.Variants).ChildRules(variant =>
        {
            variant.RuleFor(v => v.SizeCode)
                .NotEmpty().WithMessage("El código de talla es requerido.")
                .MaximumLength(20);

            variant.RuleFor(v => v.ColorName)
                .NotEmpty().WithMessage("El nombre del color es requerido.")
                .MaximumLength(50);

            variant.RuleFor(v => v.ColorHex)
                .NotEmpty().WithMessage("El código hex del color es requerido.")
                .Matches(@"^#[0-9A-Fa-f]{6}$").WithMessage("El código hex debe tener formato #RRGGBB.");

            variant.RuleFor(v => v.CostPrice)
                .GreaterThanOrEqualTo(0).WithMessage("El costo de material no puede ser negativo.");
        });
    }
}
