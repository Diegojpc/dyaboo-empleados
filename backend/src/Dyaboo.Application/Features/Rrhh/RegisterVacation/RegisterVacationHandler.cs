using Dyaboo.Application.Features.Rrhh.GetVacationBalances;
using Dyaboo.Application.Interfaces;
using Dyaboo.Domain.Entities;
using Dyaboo.Domain.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Dyaboo.Application.Features.Rrhh.RegisterVacation;

public class RegisterVacationHandler(IApplicationDbContext db)
    : IRequestHandler<RegisterVacationCommand, VacationPeriodResult>
{
    public async Task<VacationPeriodResult> Handle(
        RegisterVacationCommand request,
        CancellationToken cancellationToken)
    {
        var employee = await db.Employees
            .FirstOrDefaultAsync(e => e.Id == request.EmployeeId, cancellationToken)
            ?? throw new KeyNotFoundException($"Empleado '{request.EmployeeId}' no encontrado.");

        if (!employee.IsActive)
            throw new InvalidOperationException(
                $"No se pueden registrar vacaciones para {employee.FullName}: el empleado está inactivo.");

        if (request.EndDate < request.StartDate)
            throw new ArgumentException("La fecha final no puede ser anterior a la inicial.");

        // Solape con periodos existentes del empleado
        var overlaps = await db.VacationPeriods
            .AnyAsync(p => p.EmployeeId == employee.Id
                && p.StartDate <= request.EndDate
                && request.StartDate <= p.EndDate, cancellationToken);

        if (overlaps)
            throw new InvalidOperationException(
                $"{employee.FullName} ya tiene un periodo de vacaciones que se cruza con esas fechas.");

        // Días hábiles: excluye domingos y festivos (sábado cuenta)
        var holidays = await db.Holidays
            .AsNoTracking()
            .Where(h => h.Date >= request.StartDate && h.Date <= request.EndDate)
            .Select(h => h.Date)
            .ToListAsync(cancellationToken);

        var businessDays = VacationCalculator.CountBusinessDays(
            request.StartDate, request.EndDate, holidays.ToHashSet());

        if (businessDays == 0)
            throw new InvalidOperationException(
                "El periodo seleccionado no contiene ningún día hábil.");

        // Saldo disponible
        var taken = await db.VacationPeriods
            .Where(p => p.EmployeeId == employee.Id)
            .SumAsync(p => p.BusinessDays, cancellationToken);

        var accrued = VacationCalculator.AccruedDays(employee.HireDate, DateOnly.FromDateTime(DateTime.Today));
        var balance = accrued - taken;

        if (businessDays > balance)
            throw new InvalidOperationException(
                $"Saldo insuficiente: {employee.FullName} tiene {balance:0.##} días disponibles " +
                $"y el periodo requiere {businessDays}.");

        var period = VacationPeriod.Create(
            employee, request.StartDate, request.EndDate, businessDays, request.Notes);

        db.VacationPeriods.Add(period);
        await db.SaveChangesAsync(cancellationToken);

        return new VacationPeriodResult(
            period.Id, period.StartDate, period.EndDate, period.BusinessDays, period.Notes);
    }
}
