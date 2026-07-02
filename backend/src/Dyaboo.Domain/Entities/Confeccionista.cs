namespace Dyaboo.Domain.Entities;

/// <summary>Taller/micro-empresa aliada que confecciona las prendas cortadas.</summary>
public class Confeccionista : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string ContactName { get; private set; } = string.Empty;
    public string Phone { get; private set; } = string.Empty;
    public string City { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;

    private Confeccionista() { }  // requerido por EF Core

    public static Confeccionista Create(string name, string contactName, string phone, string city)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(phone);

        return new Confeccionista
        {
            Name        = name.Trim(),
            ContactName = contactName?.Trim() ?? string.Empty,
            Phone       = phone.Trim(),
            City        = city?.Trim() ?? string.Empty
        };
    }

    public void Deactivate()
    {
        IsActive = false;
        MarkUpdated();
    }
}
