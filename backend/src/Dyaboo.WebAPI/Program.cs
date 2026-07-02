using System.Text;
using System.Threading.RateLimiting;
using Dyaboo.Application;
using Dyaboo.Infrastructure;
using Dyaboo.Infrastructure.Persistence;
using Dyaboo.Infrastructure.Services;
using Dyaboo.WebAPI.Middleware;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger con soporte para Bearer token
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Dyaboo ERP API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name         = "Authorization",
        Type         = SecuritySchemeType.Http,
        Scheme       = "bearer",
        BearerFormat = "JWT",
        In           = ParameterLocation.Header,
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            []
        }
    });
});

// JWT Authentication — key must come from env var (Jwt__Key) in Docker / production
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey))
    throw new InvalidOperationException(
        "Jwt:Key no está configurado. Define JWT_SECRET_KEY en el archivo .env (mínimo 32 caracteres).");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = builder.Configuration["Jwt:Issuer"],
            ValidAudience            = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew                = TimeSpan.Zero, // no tolerance window — tokens expire exactly on time
        };
    });

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// CORS — restrict to known origins; AllowAnyOrigin is only for dev fallback
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? ["http://localhost:3000"];

builder.Services.AddCors(o => o.AddPolicy("AppPolicy", p =>
    p.WithOrigins(allowedOrigins)
     .AllowAnyHeader()
     .AllowAnyMethod()
     .AllowCredentials()));

// Rate limiting — OWASP recommendation, .NET 8 built-in
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Login: strict fixed window — 10 attempts / 15 min per IP
    options.AddFixedWindowLimiter("login", o =>
    {
        o.Window           = TimeSpan.FromMinutes(15);
        o.PermitLimit      = 10;
        o.QueueLimit       = 0;
        o.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });

    // General API: sliding window — 300 requests / min per IP
    options.AddSlidingWindowLimiter("api", o =>
    {
        o.Window           = TimeSpan.FromMinutes(1);
        o.PermitLimit      = 300;
        o.SegmentsPerWindow = 6;
        o.QueueLimit       = 0;
        o.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });
});

builder.Services.AddHealthChecks();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DyabooDbContext>();
    db.Database.Migrate();
    await WarehouseSeeder.SeedAsync(db);
    await UserSeeder.SeedAsync(db);
    await ConfeccionistaSeeder.SeedAsync(db);
    await CustomerSeeder.SeedAsync(db);
    await HolidaySeeder.SeedAsync(db);
    await EmployeeSeeder.SeedAsync(db);
    await RrhhUserSeeder.SeedAsync(db);

    var minio = scope.ServiceProvider.GetRequiredService<MinioInitializer>();
    await minio.InitializeAsync();
}

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Dyaboo ERP v1"));

app.MapHealthChecks("/health");

// Security middleware — must be before authentication
app.UseSecurityHeaders();
app.UseRateLimiter();
app.UseCors("AppPolicy");
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();

// Global api rate limit; login endpoint overrides with stricter "login" policy via [EnableRateLimiting]
app.MapControllers().RequireRateLimiting("api");

app.Run();
