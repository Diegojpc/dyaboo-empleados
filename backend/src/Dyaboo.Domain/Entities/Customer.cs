using Dyaboo.Domain.Enums;

namespace Dyaboo.Domain.Entities;

/// <summary>Cliente de distribución: tienda propia o mayorista externo.</summary>
public class Customer : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public CustomerType Type { get; private set; }
    public string ContactName { get; private set; } = string.Empty;
    public string Phone { get; private set; } = string.Empty;
    public string City { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;

    private Customer() { }  // requerido por EF Core

    public static Customer Create(string name, CustomerType type, string contactName, string phone, string city)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new Customer
        {
            Name        = name.Trim(),
            Type        = type,
            ContactName = contactName?.Trim() ?? string.Empty,
            Phone       = phone?.Trim() ?? string.Empty,
            City        = city?.Trim() ?? string.Empty
        };
    }

    public void Deactivate()
    {
        IsActive = false;
        MarkUpdated();
    }
}
