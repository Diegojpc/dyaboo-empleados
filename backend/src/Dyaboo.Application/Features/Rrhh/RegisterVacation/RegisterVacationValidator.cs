using FluentValidation;

namespace Dyaboo.Application.Features.Rrhh.RegisterVacation;

public class RegisterVacationValidator : AbstractValidator<RegisterVacationCommand>
{
    public RegisterVacationValidator()
    {
        RuleFor(x => x.EmployeeId).NotEmpty();

        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate)
            .WithMessage("La fecha final no puede ser anterior a la inicial.");

        RuleFor(x => x.Notes).MaximumLength(500);
    }
}
