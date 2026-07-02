using FluentValidation;

namespace Dyaboo.Application.Features.Rrhh.RegisterOvertime;

public class RegisterOvertimeValidator : AbstractValidator<RegisterOvertimeCommand>
{
    public RegisterOvertimeValidator()
    {
        RuleFor(x => x.EmployeeId).NotEmpty();
        RuleFor(x => x.Type).IsInEnum();

        RuleFor(x => x.Hours)
            .GreaterThan(0).WithMessage("Las horas deben ser mayores a cero.")
            .LessThanOrEqualTo(24).WithMessage("Las horas no pueden superar 24 en un día.");

        RuleFor(x => x.Notes).MaximumLength(500);
    }
}
