using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Minio;
using Minio.DataModel.Args;

namespace Dyaboo.Infrastructure.Services;

/// <summary>
/// Crea el bucket y aplica política pública de lectura en el arranque.
/// </summary>
public sealed class MinioInitializer(IMinioClient client, IConfiguration config, ILogger<MinioInitializer> logger)
{
    private static readonly string PublicReadPolicy = """
        {{
          "Version": "2012-10-17",
          "Statement": [{{
            "Effect": "Allow",
            "Principal": {{"AWS": ["*"]}},
            "Action": ["s3:GetObject"],
            "Resource": ["arn:aws:s3:::{0}/*"]
          }}]
        }}
        """;

    public async Task InitializeAsync(CancellationToken ct = default)
    {
        var bucket = config["Minio:Bucket"] ?? "dyaboo-assets";

        try
        {
            var exists = await client.BucketExistsAsync(
                new BucketExistsArgs().WithBucket(bucket), ct);

            if (!exists)
            {
                await client.MakeBucketAsync(
                    new MakeBucketArgs().WithBucket(bucket), ct);
                logger.LogInformation("MinIO: bucket '{Bucket}' creado.", bucket);
            }

            var policy = string.Format(PublicReadPolicy, bucket);
            await client.SetPolicyAsync(
                new SetPolicyArgs().WithBucket(bucket).WithPolicy(policy), ct);

            logger.LogInformation("MinIO: bucket '{Bucket}' listo con lectura pública.", bucket);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "MinIO: no se pudo inicializar el bucket '{Bucket}'.", bucket);
        }
    }
}
