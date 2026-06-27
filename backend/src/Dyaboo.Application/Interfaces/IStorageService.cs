namespace Dyaboo.Application.Interfaces;

public interface IStorageService
{
    /// <summary>Sube un archivo y devuelve el nombre guardado (key).</summary>
    Task<string> UploadAsync(Stream stream, string fileName, string contentType, CancellationToken ct = default);

    /// <summary>Elimina un archivo por su nombre guardado.</summary>
    Task DeleteAsync(string fileName, CancellationToken ct = default);

    /// <summary>Devuelve la URL pública del archivo.</summary>
    string GetPublicUrl(string fileName);
}
