using Dyaboo.Application.Features.SAG.CalculateProductionCost;
using Dyaboo.Application.Features.SAG.GetFinancialInventory;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dyaboo.WebAPI.Controllers;

[ApiController]
[Route("api/sag/[controller]")]
[Authorize(Roles = "Ceo,Socio,LiderProduccion")]
public class ProductionCostsController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Calcula el costo de producción de una referencia textil.
    /// Incluye desglose por variante: material, mano de obra y CIF (overhead).
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ProductionCostResult), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Calculate(
        [FromBody] CalculateProductionCostCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await mediator.Send(command, cancellationToken);
            return CreatedAtAction(nameof(Calculate), new { id = result.OrderId }, result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}

[ApiController]
[Route("api/sag/[controller]")]
[Authorize(Roles = "Ceo,Socio,LiderProduccion")]
public class FinancialInventoryController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Retorna el inventario financiero general: stock valorizado por referencia y variante.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(FinancialInventoryResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetFinancialInventoryQuery(), cancellationToken);
        return Ok(result);
    }
}
