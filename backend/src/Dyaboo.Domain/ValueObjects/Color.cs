namespace Dyaboo.Domain.ValueObjects;

public sealed record Color
{
    public string Name { get; }
    public string HexCode { get; }  // formato #RRGGBB

    private Color(string name, string hexCode)
    {
        Name = name;
        HexCode = hexCode;
    }

    public static Color From(string name, string hexCode)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("El nombre del color no puede estar vacío.");

        var normalized = hexCode.Trim().ToUpper();
        if (!System.Text.RegularExpressions.Regex.IsMatch(normalized, @"^#[0-9A-F]{6}$"))
            throw new ArgumentException($"Código hex inválido: '{hexCode}'. Formato esperado: #RRGGBB.");

        return new Color(name.Trim(), normalized);
    }

    public override string ToString() => $"{Name} ({HexCode})";
}
