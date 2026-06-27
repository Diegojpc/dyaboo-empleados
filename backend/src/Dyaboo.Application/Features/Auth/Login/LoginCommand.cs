using MediatR;

namespace Dyaboo.Application.Features.Auth.Login;

public record LoginCommand(string Email, string Password) : IRequest<LoginResult>;

public record LoginResult(string Token, Guid UserId, string Name, string Email, string Role);
