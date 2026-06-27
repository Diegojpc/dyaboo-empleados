using Dyaboo.Application.Features.WMS.AssignStock;
using Dyaboo.Application.Features.WMS.GetWarehouseStatus;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dyaboo.WebAPI.Controllers;

[ApiController]
[Route("api/wms/[controller]")]
[Authorize(Roles = "Ceo,Socio,LiderBodega,Operario")]
public class StockAssignmentsController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Asigna automáticamente el stock entrante a ubicaciones físicas de bodega (Pasillo-Estante-Nivel).
    /// Llena ubicaciones secuencialmente y actualiza el inventario financiero del SAG.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(AssignStockResult), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Assign(
        [FromBody] AssignStockCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await mediator.Send(command, cancellationToken);
            return CreatedAtAction(nameof(Assign), result);
        }
        catch (KeyNotFoundException ex)   { return NotFound(new { error = ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
        catch (ArgumentException ex)      { return BadRequest(new { error = ex.Message }); }
    }
}

[ApiController]
[Route("api/wms/[controller]")]
[Authorize(Roles = "Ceo,Socio,LiderBodega,Operario")]
public class WarehouseStatusController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Estado en tiempo real de la bodega: ocupación por pasillo, estante y nivel.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(WarehouseStatusResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetWarehouseStatusQuery(), cancellationToken);
        return Ok(result);
    }
}
