using Dyaboo.Application.Features.PLM.CreateProductReference;
using Dyaboo.Application.Features.PLM.GetProductReferences;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dyaboo.WebAPI.Controllers;

[ApiController]
[Route("api/plm/[controller]")]
[Authorize(Roles = "Ceo,Socio,LiderPlm,Disenadora,Vendedor")]
public class ProductReferencesController(IMediator mediator) : ControllerBase
{
    /// <summary>Lista todas las referencias textiles activas con sus variantes.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ProductReferenceListItem>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetProductReferencesQuery(), cancellationToken);
        return Ok(result);
    }

    /// <summary>Crea una nueva referencia textil con sus variaciones de talla y color.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ProductReferenceResult), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateProductReferenceCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await mediator.Send(command, cancellationToken);
            return CreatedAtAction(nameof(GetAll), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex) { return Conflict(new { error = ex.Message }); }
        catch (ArgumentException ex)         { return BadRequest(new { error = ex.Message }); }
    }
}
