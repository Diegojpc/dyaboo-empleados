using Dyaboo.Application.Features.Rrhh.GetOvertime;
using Dyaboo.Application.Interfaces;
using Dyaboo.Domain.Entities;
using Dyaboo.Domain.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Dyaboo.Application.Features.Rrhh.RegisterOvertime;

public class RegisterOvertimeHandler(IApplicationDbContext db)
    : IRequestHandler<RegisterOvertimeCommand, OvertimeResult>
{
    public async Task<OvertimeResult> Handle(
        RegisterOvertimeCommand request,
        CancellationToken cancellationToken)
    {
        var employee = await db.Employees
            .FirstOrDefaultAsync(e => e.Id == request.EmployeeId, cancellationToken)
            ?? throw new KeyNotFoundException($"Empleado '{request.EmployeeId}' no encontrado.");

        if (!employee.IsActive)
            throw new InvalidOperationException(
                $"No se pueden registrar novedades para {employee.FullName}: el empleado está inactivo.");

        // Límite legal: máx 2h extra por día (RIT Art. 31), sumando lo ya registrado
        if (SurchargeCalculator.IsExtraHours(request.Type))
        {
            var extraTypes = new[]
            {
                Domain.Enums.OvertimeType.ExtraDiurna,
                Domain.Enums.OvertimeType.ExtraNocturna,
                Domain.Enums.OvertimeType.ExtraDiurnaDominical,
                Domain.Enums.OvertimeType.ExtraNocturnaDominical,
            };

            var alreadyRegistered = await db.OvertimeEntries
                .Where(o => o.EmployeeId == employee.Id
                    && o.Date == request.Date
                    && extraTypes.Contains(o.Type))
                .SumAsync(o => o.Hours, cancellationToken);

            if (alreadyRegistered + request.Hours > OvertimeEntry.MaxDailyExtraHours)
                throw new InvalidOperationException(
                    $"Las horas extra del día no pueden superar {OvertimeEntry.MaxDailyExtraHours:0} " +
                    $"(RIT Art. 31). {employee.FullName} ya tiene {alreadyRegistered:0.##} h registradas el {request.Date:yyyy-MM-dd}.");
        }

        var entry = OvertimeEntry.Create(employee, request.Date, request.Type, request.Hours, request.Notes);

        db.OvertimeEntries.Add(entry);
        await db.SaveChangesAsync(cancellationToken);

        return GetOvertimeHandler.MapToResult(entry);
    }
}
