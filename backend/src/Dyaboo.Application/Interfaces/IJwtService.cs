using Dyaboo.Domain.Entities;

namespace Dyaboo.Application.Interfaces;

public interface IJwtService
{
    string GenerateToken(User user);
}
