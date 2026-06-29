using Dyaboo.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Dyaboo.Application.Features.Auth.Login;

public class LoginHandler(
    IApplicationDbContext db,
    IJwtService jwt,
    IPasswordHasher hasher,
    ILogger<LoginHandler> logger) : IRequestHandler<LoginCommand, LoginResult>
{
    public async Task<LoginResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.ToLowerInvariant();

        var user = await db.Users
            .FirstOrDefaultAsync(u => u.Email == email && u.IsActive, cancellationToken);

        if (user is null || !hasher.Verify(request.Password, user.PasswordHash))
        {
            logger.LogWarning("AUTH_FAIL: intento de login fallido para email={Email} at={At}",
                email, DateTimeOffset.UtcNow);
            throw new UnauthorizedAccessException("Credenciales inválidas.");
        }

        logger.LogInformation("AUTH_OK: login exitoso userId={UserId} role={Role} at={At}",
            user.Id, user.Role, DateTimeOffset.UtcNow);

        return new LoginResult(jwt.GenerateToken(user), user.Id, user.Name, user.Email, user.Role.ToString());
    }
}
