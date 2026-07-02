namespace Dyaboo.Domain.Entities;

/// <summary>Festivo colombiano (Ley Emiliani). Fecha única.</summary>
public class Holiday : BaseEntity
{
    public DateOnly Date { get; private set; }
    public string Name { get; private set; } = string.Empty;

    private Holiday() { }  // requerido por EF Core

    public static Holiday Create(DateOnly date, string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new Holiday
        {
            Date = date,
            Name = name.Trim()
        };
    }
}
