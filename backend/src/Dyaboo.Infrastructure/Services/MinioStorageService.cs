using Dyaboo.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Minio;
using Minio.DataModel.Args;

namespace Dyaboo.Infrastructure.Services;

public sealed class MinioStorageService : IStorageService
{
    private readonly IMinioClient _client;
    private readonly string _bucket;
    private readonly string _publicUrl;

    public MinioStorageService(IMinioClient client, IConfiguration config)
    {
        _client    = client;
        _bucket    = config["Minio:Bucket"]    ?? "dyaboo-assets";
        _publicUrl = config["Minio:PublicUrl"] ?? "http://localhost:9000";
    }

    public async Task<string> UploadAsync(
        Stream stream, string fileName, string contentType, CancellationToken ct = default)
    {
        var args = new PutObjectArgs()
            .WithBucket(_bucket)
            .WithObject(fileName)
            .WithStreamData(stream)
            .WithObjectSize(stream.Length)
            .WithContentType(contentType);

        await _client.PutObjectAsync(args, ct);
        return fileName;
    }

    public async Task DeleteAsync(string fileName, CancellationToken ct = default)
    {
        var args = new RemoveObjectArgs()
            .WithBucket(_bucket)
            .WithObject(fileName);

        await _client.RemoveObjectAsync(args, ct);
    }

    public string GetPublicUrl(string fileName) =>
        $"{_publicUrl.TrimEnd('/')}/{_bucket}/{fileName}";
}
