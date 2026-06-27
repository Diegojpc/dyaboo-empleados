namespace Dyaboo.Domain.ValueObjects;

/// <summary>Código canónico de ubicación física: Pasillo-Estante-Nivel. Ej: "A-03-2"</summary>
public sealed record WarehouseLocationCode
{
    public string Aisle { get; }    // A, B, C…
    public int Shelf { get; }       // 1-99
    public int Level { get; }       // 1-9
    public string Code { get; }     // "A-03-2"

    private WarehouseLocationCode(string aisle, int shelf, int level)
    {
        Aisle = aisle;
        Shelf = shelf;
        Level = level;
        Code  = $"{aisle}-{shelf:D2}-{level}";
    }

    public static WarehouseLocationCode From(string aisle, int shelf, int level)
    {
        if (string.IsNullOrWhiteSpace(aisle) || aisle.Length > 3)
            throw new ArgumentException("El pasillo debe ser entre 1 y 3 caracteres.");
        if (shelf is < 1 or > 99)
            throw new ArgumentException("El estante debe estar entre 1 y 99.");
        if (level is < 1 or > 9)
            throw new ArgumentException("El nivel debe estar entre 1 y 9.");

        return new WarehouseLocationCode(aisle.Trim().ToUpper(), shelf, level);
    }

    public override string ToString() => Code;
}
