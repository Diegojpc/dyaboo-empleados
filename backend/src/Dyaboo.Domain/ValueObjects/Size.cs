namespace Dyaboo.Domain.ValueObjects;

public sealed record Size
{
    public string Code { get; }  // XS, S, M, L, XL, XXL  o numérico  28, 30, 32…

    private static readonly HashSet<string> _standard =
        ["XS", "S", "M", "L", "XL", "XXL", "XXXL"];

    private Size(string code) => Code = code;

    public static Size From(string code)
    {
        var normalized = code.Trim().ToUpper();
        if (string.IsNullOrWhiteSpace(normalized))
            throw new ArgumentException("La talla no puede estar vacía.");
        return new Size(normalized);
    }

    public bool IsAlphanumeric => _standard.Contains(Code);
    public override string ToString() => Code;
}
