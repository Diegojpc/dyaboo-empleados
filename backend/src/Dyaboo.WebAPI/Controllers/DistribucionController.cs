using Dyaboo.Application.Features.Distribucion.CreateCustomer;
using Dyaboo.Application.Features.Distribucion.CreateSalesOrder;
using Dyaboo.Application.Features.Distribucion.DispatchSalesOrder;
using Dyaboo.Application.Features.Distribucion.GetCustomers;
using Dyaboo.Application.Features.Distribucion.GetSalesOrders;
using Dyaboo.Application.Features.Distribucion.TransitionSalesOrder;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dyaboo.WebAPI.Controllers;

[ApiController]
[Route("api/distribucion/[controller]")]
[Authorize(Roles = "Ceo,Socio,LiderDistribucion")]
public class CustomersController(IMediator mediator) : ControllerBase
{
    /// <summary>Clientes: tiendas propias y mayoristas externos.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CustomerResult>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
        => Ok(await mediator.Send(new GetCustomersQuery(), cancellationToken));

    /// <summary>Registra un nuevo cliente.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(CustomerResult), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateCustomerCommand command,
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
[Route("api/distribucion/[controller]")]
[Authorize(Roles = "Ceo,Socio,LiderDistribucion")]
public class SalesOrdersController(IMediator mediator) : ControllerBase
{
    /// <summary>Pedidos de venta con sus ítems y estado.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<SalesOrderResult>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
        => Ok(await mediator.Send(new GetSalesOrdersQuery(), cancellationToken));

    /// <summary>Crea un pedido en borrador para un cliente.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(SalesOrderResult), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create(
        [FromBody] CreateSalesOrderCommand command,
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

    /// <summary>Confirma un pedido en borrador.</summary>
    [HttpPost("{id:guid}/confirm")]
    [ProducesResponseType(typeof(SalesOrderResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<IActionResult> Confirm(Guid id, CancellationToken ct)
        => Transition(id, SalesOrderAction.Confirm, ct);

    /// <summary>
    /// Despacha un pedido confirmado: descuenta stock físico de bodega en orden
    /// Pasillo-Estante-Nivel y devuelve la lista de picking.
    /// </summary>
    [HttpPost("{id:guid}/dispatch")]
    [ProducesResponseType(typeof(DispatchResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Dispatch(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await mediator.Send(new DispatchSalesOrderCommand(id), cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)      { return NotFound(new { error = ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
        catch (ArgumentException ex)         { return BadRequest(new { error = ex.Message }); }
    }

    /// <summary>Marca un pedido despachado como entregado.</summary>
    [HttpPost("{id:guid}/deliver")]
    [ProducesResponseType(typeof(SalesOrderResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<IActionResult> Deliver(Guid id, CancellationToken ct)
        => Transition(id, SalesOrderAction.Deliver, ct);

    /// <summary>Cancela un pedido en borrador o confirmado.</summary>
    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(typeof(SalesOrderResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<IActionResult> Cancel(Guid id, CancellationToken ct)
        => Transition(id, SalesOrderAction.Cancel, ct);

    private async Task<IActionResult> Transition(
        Guid id, SalesOrderAction action, CancellationToken cancellationToken)
    {
        try
        {
            var result = await mediator.Send(
                new TransitionSalesOrderCommand(id, action), cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)      { return NotFound(new { error = ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
        catch (ArgumentException ex)         { return BadRequest(new { error = ex.Message }); }
    }
}
