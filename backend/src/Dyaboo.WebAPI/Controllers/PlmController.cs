using Dyaboo.Application.Features.PLM.CreateProductReference;
using Dyaboo.Application.Features.PLM.GetProductReferences;
using Dyaboo.Application.Interfaces;
using Dyaboo.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dyaboo.WebAPI.Controllers;

// Roles de escritura a nivel de acción; la lectura del catálogo es más amplia
// porque Producción, Bodega y Distribución la necesitan para su operación.
[ApiController]
[Route("api/plm/[controller]")]
[Authorize]
public class ProductReferencesController(
    IMediator mediator,
    IApplicationDbContext db,
    IStorageService storage) : ControllerBase
{
    private static readonly string[] AllowedTypes = ["image/jpeg", "image/png", "image/webp"];
    private const long MaxFileSize = 10 * 1024 * 1024; // 10 MB

    // Magic bytes: JPEG=FF D8 FF, PNG=89 50 4E 47, WebP=52 49 46 46
    private static async Task<bool> IsValidImageSignatureAsync(IFormFile file)
    {
        var buffer = new byte[12];
        await using var stream = file.OpenReadStream();
        var read = await stream.ReadAsync(buffer.AsMemory(0, 12));
        if (read < 4) return false;

        bool isJpeg = buffer[0] == 0xFF && buffer[1] == 0xD8 && buffer[2] == 0xFF;
        bool isPng  = buffer[0] == 0x89 && buffer[1] == 0x50 && buffer[2] == 0x4E && buffer[3] == 0x47;
        bool isWebp = buffer[0] == 0x52 && buffer[1] == 0x49 && buffer[2] == 0x46 && buffer[3] == 0x46
                   && read >= 12
                   && buffer[8] == 0x57 && buffer[9] == 0x45 && buffer[10] == 0x42 && buffer[11] == 0x50;
        return isJpeg || isPng || isWebp;
    }

    [HttpGet]
    [Authorize(Roles = "Ceo,Socio,LiderPlm,Disenadora,Vendedor,LiderProduccion,LiderBodega,Operario,LiderDistribucion")]
    [ProducesResponseType(typeof(IReadOnlyList<ProductReferenceListItem>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await mediator.Send(new GetProductReferencesQuery(), ct);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Ceo,Socio,LiderPlm,Disenadora,Vendedor")]
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
    [Authorize(Roles = "Ceo,Socio,LiderPlm,Disenadora,Vendedor")]
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

        if (!await IsValidImageSignatureAsync(file))
            return BadRequest(new { error = "El contenido del archivo no corresponde a una imagen válida." });

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
    [Authorize(Roles = "Ceo,Socio,LiderPlm,Disenadora,Vendedor")]
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
