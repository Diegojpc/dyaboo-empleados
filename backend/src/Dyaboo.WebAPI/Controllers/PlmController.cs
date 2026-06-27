using Dyaboo.Application.Features.PLM.CreateProductReference;
using Dyaboo.Application.Features.PLM.GetProductReferences;
using Dyaboo.Application.Interfaces;
using Dyaboo.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dyaboo.WebAPI.Controllers;

[ApiController]
[Route("api/plm/[controller]")]
[Authorize(Roles = "Ceo,Socio,LiderPlm,Disenadora,Vendedor")]
public class ProductReferencesController(
    IMediator mediator,
    IApplicationDbContext db,
    IStorageService storage) : ControllerBase
{
    private static readonly string[] AllowedTypes = ["image/jpeg", "image/png", "image/webp"];
    private const long MaxFileSize = 10 * 1024 * 1024; // 10 MB

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ProductReferenceListItem>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await mediator.Send(new GetProductReferencesQuery(), ct);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ProductReferenceResult), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(
        [FromBody] CreateProductReferenceCommand command,
        CancellationToken ct)
    {
        try
        {
            var result = await mediator.Send(command, ct);
            return CreatedAtAction(nameof(GetAll), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex) { return Conflict(new { error = ex.Message }); }
        catch (ArgumentException ex)         { return BadRequest(new { error = ex.Message }); }
    }

    /// <summary>Sube una imagen a MinIO y registra el metadata en la BD.</summary>
    [HttpPost("{id:guid}/images")]
    [RequestSizeLimit(MaxFileSize + 1024)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UploadImage(
        Guid id, IFormFile file, CancellationToken ct)
    {
        var reference = await db.ProductReferences
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == id && p.IsActive, ct);

        if (reference is null)
            return NotFound(new { error = "Referencia no encontrada." });

        if (file is null || file.Length == 0)
            return BadRequest(new { error = "No se adjuntó ningún archivo." });

        if (file.Length > MaxFileSize)
            return BadRequest(new { error = "El archivo supera el límite de 10 MB." });

        if (!AllowedTypes.Contains(file.ContentType.ToLower()))
            return BadRequest(new { error = "Solo se aceptan imágenes JPG, PNG o WebP." });

        var ext      = Path.GetExtension(file.FileName);
        var fileName = $"{Guid.NewGuid()}{ext}";

        await using var stream = file.OpenReadStream();
        await storage.UploadAsync(stream, fileName, file.ContentType, ct);

        var image = ProductImage.Create(
            id, fileName, file.FileName, file.ContentType,
            file.Length, reference.Images.Count);

        await db.ProductImages.AddAsync(image, ct);
        await db.SaveChangesAsync(ct);

        return Created(storage.GetPublicUrl(fileName), new
        {
            image.Id,
            image.FileName,
            image.OriginalName,
            Url       = storage.GetPublicUrl(fileName),
            image.SortOrder,
        });
    }

    /// <summary>Elimina la imagen de MinIO y su metadata de la BD.</summary>
    [HttpDelete("{id:guid}/images/{imageId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteImage(Guid id, Guid imageId, CancellationToken ct)
    {
        var image = await db.ProductImages
            .FirstOrDefaultAsync(i => i.Id == imageId && i.ProductReferenceId == id, ct);

        if (image is null) return NotFound(new { error = "Imagen no encontrada." });

        await storage.DeleteAsync(image.FileName, ct);

        db.ProductImages.Remove(image);
        await db.SaveChangesAsync(ct);

        return NoContent();
    }
}
