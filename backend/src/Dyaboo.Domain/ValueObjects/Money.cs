namespace Dyaboo.Domain.ValueObjects;

public sealed record Money
{
    public decimal Amount { get; }
    public string Currency { get; }  // ISO 4217: "COP", "USD"

    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public static Money Of(decimal amount, string currency = "COP")
    {
        if (amount < 0) throw new ArgumentException("El monto no puede ser negativo.");
        if (string.IsNullOrWhiteSpace(currency)) throw new ArgumentException("La moneda es requerida.");
        return new Money(Math.Round(amount, 4), currency.ToUpper());
    }

    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException($"No se pueden sumar monedas distintas: {Currency} y {other.Currency}.");
        return new Money(Amount + other.Amount, Currency);
    }

    public Money Multiply(int factor) => new(Amount * factor, Currency);
    public Money ApplyPercentage(decimal pct) => new(Math.Round(Amount * pct / 100, 4), Currency);

    public override string ToString() => $"{Amount:N2} {Currency}";
}
