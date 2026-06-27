using Dyaboo.Domain.Enums;

namespace Dyaboo.Domain.Entities;

public class User : BaseEntity
{
    public string Name { get; private set; } = default!;
    public string Email { get; private set; } = default!;
    public string PasswordHash { get; private set; } = default!;
    public UserRole Role { get; private set; }
    public bool IsActive { get; private set; } = true;

    private User() { }

    public static User Create(string name, string email, string passwordHash, UserRole role) =>
        new()
        {
            Name = name,
            Email = email.ToLowerInvariant(),
            PasswordHash = passwordHash,
            Role = role,
            IsActive = true,
        };

    public void Deactivate() => IsActive = false;
}
