namespace Dyaboo.WebAPI.Middleware;

public class SecurityHeadersMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var headers = context.Response.Headers;

        // Remove server fingerprinting headers
        context.Response.OnStarting(() =>
        {
            headers.Remove("Server");
            headers.Remove("X-Powered-By");
            headers.Remove("X-AspNet-Version");
            headers.Remove("X-AspNetMvc-Version");
            return Task.CompletedTask;
        });

        // Prevent MIME-type sniffing
        headers["X-Content-Type-Options"] = "nosniff";

        // Deny framing (clickjacking)
        headers["X-Frame-Options"] = "DENY";

        // XSS filter for legacy browsers
        headers["X-XSS-Protection"] = "1; mode=block";

        // Limit referrer info leakage
        headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

        // Disable browser features not needed by the API
        headers["Permissions-Policy"] = "geolocation=(), camera=(), microphone=(), payment=()";

        // CSP for the API (only serves JSON, no HTML/scripts)
        headers["Content-Security-Policy"] = "default-src 'none'; frame-ancestors 'none'";

        await next(context);
    }
}

public static class SecurityHeadersMiddlewareExtensions
{
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
        => app.UseMiddleware<SecurityHeadersMiddleware>();
}
