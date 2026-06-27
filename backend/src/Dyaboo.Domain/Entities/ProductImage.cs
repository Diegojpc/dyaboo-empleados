namespace Dyaboo.Domain.Entities;

public class ProductImage : BaseEntity
{
    public Guid ProductReferenceId { get; private set; }
    public string FileName     { get; private set; } = string.Empty;
    public string OriginalName { get; private set; } = string.Empty;
    public string ContentType  { get; private set; } = string.Empty;
    public long   FileSize     { get; private set; }
    public int    SortOrder    { get; private set; }

    public ProductReference ProductReference { get; private set; } = null!;

    private ProductImage() { }

    public static ProductImage Create(
        Guid   productReferenceId,
        string fileName,
        string originalName,
        string contentType,
        long   fileSize,
        int    sortOrder = 0) => new()
    {
        ProductReferenceId = productReferenceId,
        FileName           = fileName,
        OriginalName       = originalName,
        ContentType        = contentType,
        FileSize           = fileSize,
        SortOrder          = sortOrder,
    };
}
