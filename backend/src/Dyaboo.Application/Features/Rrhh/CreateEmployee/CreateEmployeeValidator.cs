using FluentValidation;

namespace Dyaboo.Application.Features.Rrhh.CreateEmployee;

public class CreateEmployeeValidator : AbstractValidator<CreateEmployeeCommand>
{
    public CreateEmployeeValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(150);

        RuleFor(x => x.DocumentNumber)
            .NotEmpty().WithMessage("La cédula es obligatoria.")
            .MaximumLength(20);

        RuleFor(x => x.JobTitle)
            .NotEmpty().WithMessage("El cargo es obligatorio.")
            .MaximumLength(100);

        RuleFor(x => x.Area).IsInEnum();

        RuleFor(x => x.MonthlySalary)
            .GreaterThan(0).WithMessage("El salario mensual debe ser mayor a cero.");

        RuleFor(x => x.WeeklyHours)
            .InclusiveBetween(1, 60).WithMessage("La jornada semanal debe estar entre 1 y 60 horas.");
    }
}
