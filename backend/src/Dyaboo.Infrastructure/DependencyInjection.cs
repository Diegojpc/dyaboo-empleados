using Dyaboo.Application.Interfaces;
using Dyaboo.Infrastructure.Persistence;
using Dyaboo.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Minio;

namespace Dyaboo.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ── PostgreSQL ────────────────────────────────────────────────────────
        services.AddDbContext<DyabooDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsql => npgsql.MigrationsAssembly(typeof(DyabooDbContext).Assembly.FullName)));

        services.AddScoped<IApplicationDbContext>(sp =>
            sp.GetRequiredService<DyabooDbContext>());

        // ── MinIO ─────────────────────────────────────────────────────────────
        services.AddMinio(cfg =>
        {
            cfg.WithEndpoint(configuration["Minio:Endpoint"] ?? "localhost:9000")
               .WithCredentials(
                   configuration["Minio:AccessKey"] ?? "minioadmin",
                   configuration["Minio:SecretKey"] ?? "minioadmin")
               .WithSSL(bool.TryParse(configuration["Minio:UseSSL"], out var ssl) && ssl);
        });

        services.AddSingleton<IStorageService, MinioStorageService>();
        services.AddSingleton<MinioInitializer>();

        // ── Auth ──────────────────────────────────────────────────────────────
        services.AddSingleton<IJwtService, JwtService>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();

        return services;
    }
}
