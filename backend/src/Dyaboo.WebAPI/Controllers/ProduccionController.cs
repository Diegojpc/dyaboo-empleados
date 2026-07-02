using Dyaboo.Application.Features.Produccion.CompleteCuttingOrder;
using Dyaboo.Application.Features.Produccion.CreateConfeccionista;
using Dyaboo.Application.Features.Produccion.CreateCuttingOrder;
using Dyaboo.Application.Features.Produccion.CreateSewingOrder;
using Dyaboo.Application.Features.Produccion.GetConfeccionistas;
using Dyaboo.Application.Features.Produccion.GetCuttingOrders;
using Dyaboo.Application.Features.Produccion.GetSewingOrders;
using Dyaboo.Application.Features.Produccion.ReceiveSewingOrder;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dyaboo.WebAPI.Controllers;

[ApiController]
[Route("api/produccion/[controller]")]
[Authorize(Roles = "Ceo,Socio,LiderProduccion")]
public class ConfeccionistasController(IMediator mediator) : ControllerBase
{
    /// <summary>Talleres de confección aliados (satélites).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ConfeccionistaResult>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
        => Ok(await mediator.Send(new GetConfeccionistasQuery(), cancellationToken));

    /// <summary>Registra un nuevo taller de confección.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ConfeccionistaResult), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateConfeccionistaCommand command,
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
}

[ApiController]
[Route("api/produccion/[controller]")]
[Authorize(Roles = "Ceo,Socio,LiderProduccion")]
public class CuttingOrdersController(IMediator mediator) : ControllerBase
{
    /// <summary>Órdenes de corte con sus ítems por variante.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CuttingOrderResult>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
        => Ok(await mediator.Send(new GetCuttingOrdersQuery(), cancellationToken));

    /// <summary>Crea una orden de corte para una referencia con cantidades por variante.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(CuttingOrderResult), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create(
        [FromBody] CreateCuttingOrderCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await mediator.Send(command, cancellationToken);
            return CreatedAtAction(nameof(Create), result);
        }
        catch (KeyNotFoundException ex)      { return NotFound(new { error = ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
        catch (ArgumentException ex)         { return BadRequest(new { error = ex.Message }); }
    }

    /// <summary>Completa el corte registrando las unidades realmente cortadas por ítem.</summary>
    [HttpPost("{id:guid}/complete")]
    [ProducesResponseType(typeof(CuttingOrderResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Complete(
        Guid id,
        [FromBody] CompleteCuttingOrderRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await mediator.Send(
                new CompleteCuttingOrderCommand(id, request.Items), cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)      { return NotFound(new { error = ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
        catch (ArgumentException ex)         { return BadRequest(new { error = ex.Message }); }
    }

    public record CompleteCuttingOrderRequest(IReadOnlyList<CutCompletionInput> Items);
}

[ApiController]
[Route("api/produccion/[controller]")]
[Authorize(Roles = "Ceo,Socio,LiderProduccion")]
public class SewingOrdersController(IMediator mediator) : ControllerBase
{
    /// <summary>Órdenes de confección enviadas a talleres.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<SewingOrderResult>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
        => Ok(await mediator.Send(new GetSewingOrdersQuery(), cancellationToken));

    /// <summary>Envía un corte completado a un taller de confección.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(SewingOrderResult), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create(
        [FromBody] CreateSewingOrderCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await mediator.Send(command, cancellationToken);
            return CreatedAtAction(nameof(Create), result);
        }
        catch (KeyNotFoundException ex)      { return NotFound(new { error = ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
        catch (ArgumentException ex)         { return BadRequest(new { error = ex.Message }); }
    }

    /// <summary>Recibe la confección con control de calidad: aprobadas + rechazadas = enviadas.</summary>
    [HttpPost("{id:guid}/receive")]
    [ProducesResponseType(typeof(SewingOrderResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Receive(
        Guid id,
        [FromBody] ReceiveSewingOrderRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await mediator.Send(
                new ReceiveSewingOrderCommand(id, request.Items), cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)      { return NotFound(new { error = ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
        catch (ArgumentException ex)         { return BadRequest(new { error = ex.Message }); }
    }

    public record ReceiveSewingOrderRequest(IReadOnlyList<ReceptionInput> Items);
}
