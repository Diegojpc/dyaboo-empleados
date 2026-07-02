using Dyaboo.Application.Features.Rrhh.CreateEmployee;
using Dyaboo.Application.Features.Rrhh.DeactivateEmployee;
using Dyaboo.Application.Features.Rrhh.GetEmployees;
using Dyaboo.Application.Features.Rrhh.GetHolidays;
using Dyaboo.Application.Features.Rrhh.GetMonthlySummary;
using Dyaboo.Application.Features.Rrhh.GetOvertime;
using Dyaboo.Application.Features.Rrhh.GetVacationBalances;
using Dyaboo.Application.Features.Rrhh.RegisterOvertime;
using Dyaboo.Application.Features.Rrhh.RegisterVacation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dyaboo.WebAPI.Controllers;

[ApiController]
[Route("api/rrhh/[controller]")]
[Authorize(Roles = "Ceo,Socio,GestionHumana")]
public class EmployeesController(IMediator mediator) : ControllerBase
{
    /// <summary>Empleados con área/dirección del organigrama, salario y valor hora.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<EmployeeResult>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
        => Ok(await mediator.Send(new GetEmployeesQuery(), cancellationToken));

    /// <summary>Registra un empleado (cédula única).</summary>
    [HttpPost]
    [ProducesResponseType(typeof(EmployeeResult), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateEmployeeCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await mediator.Send(command, cancellationToken);
            return CreatedAtAction(nameof(Create), result);
        }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
        catch (ArgumentException ex)         { return BadRequest(new { error = ex.Message }); }
    }

    /// <summary>Desactiva un empleado (retiro).</summary>
    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType(typeof(EmployeeResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await mediator.Send(new DeactivateEmployeeCommand(id), cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException ex) { return NotFound(new { error = ex.Message }); }
    }
}

[ApiController]
[Route("api/rrhh/[controller]")]
[Authorize(Roles = "Ceo,Socio,GestionHumana")]
public class OvertimeController(IMediator mediator) : ControllerBase
{
    /// <summary>Novedades de horas, filtrables por año, mes y empleado.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<OvertimeResult>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(
        [FromQuery] int? year, [FromQuery] int? month, [FromQuery] Guid? employeeId,
        CancellationToken cancellationToken)
        => Ok(await mediator.Send(new GetOvertimeQuery(year, month, employeeId), cancellationToken));

    /// <summary>
    /// Registra una novedad de horas: calcula y congela el recargo según el tipo
    /// y la fecha (Ley 2466/2025 progresiva para dominicales/festivos).
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(OvertimeResult), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterOvertimeCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await mediator.Send(command, cancellationToken);
            return CreatedAtAction(nameof(Register), result);
        }
        catch (KeyNotFoundException ex)      { return NotFound(new { error = ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
        catch (ArgumentException ex)         { return BadRequest(new { error = ex.Message }); }
    }
}

[ApiController]
[Route("api/rrhh/[controller]")]
[Authorize(Roles = "Ceo,Socio,GestionHumana")]
public class VacationsController(IMediator mediator) : ControllerBase
{
    /// <summary>Saldos de vacaciones por empleado (causadas, disfrutadas, saldo) con sus periodos.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<VacationBalanceResult>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
        => Ok(await mediator.Send(new GetVacationBalancesQuery(), cancellationToken));

    /// <summary>
    /// Registra un periodo de vacaciones. Calcula los días hábiles (excluye
    /// domingos y festivos; el sábado cuenta) y valida solape y saldo.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(VacationPeriodResult), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterVacationCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await mediator.Send(command, cancellationToken);
            return CreatedAtAction(nameof(Register), result);
        }
        catch (KeyNotFoundException ex)      { return NotFound(new { error = ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
        catch (ArgumentException ex)         { return BadRequest(new { error = ex.Message }); }
    }
}

[ApiController]
[Route("api/rrhh/[controller]")]
[Authorize(Roles = "Ceo,Socio,GestionHumana")]
public class HolidaysController(IMediator mediator) : ControllerBase
{
    /// <summary>Festivos colombianos, filtrables por año.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<HolidayResult>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get([FromQuery] int? year, CancellationToken cancellationToken)
        => Ok(await mediator.Send(new GetHolidaysQuery(year), cancellationToken));
}

[ApiController]
[Route("api/rrhh/[controller]")]
[Authorize(Roles = "Ceo,Socio,GestionHumana")]
public class SummaryController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Resumen mensual de novedades por empleado (horas por tipo, total de
    /// recargos y vacaciones) para exportar al software contable.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<MonthlySummaryRow>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Get(
        [FromQuery] int year, [FromQuery] int month,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await mediator.Send(new GetMonthlySummaryQuery(year, month), cancellationToken));
        }
        catch (ArgumentException ex) { return BadRequest(new { error = ex.Message }); }
    }
}
