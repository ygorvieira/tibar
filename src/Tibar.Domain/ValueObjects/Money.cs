using Tibar.Domain.Exceptions;

namespace Tibar.Domain.ValueObjects;

public record Money
{
    public decimal Amount { get; }
    public string Currency { get; }

    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public static Money Create(decimal amount, string currency = "BRL")
    {
        if (amount < 0)
            throw new DomainException("O valor não pode ser negativo");

        if (string.IsNullOrWhiteSpace(currency))
            throw new DomainException("Moeda é obrigatória");

        return new Money(amount, currency.ToUpperInvariant());
    }

    public static Money operator +(Money a, Money b)
    {
        if (a.Currency != b.Currency)
            throw new DomainException("Não é possível somar moedas diferentes");

        return Create(a.Amount + b.Amount, a.Currency);
    }

    public static Money operator -(Money a, Money b)
    {
        if (a.Currency != b.Currency)
            throw new DomainException("Não é possível subtrair moedas diferentes");

        return Create(a.Amount - b.Amount, a.Currency);
    }

    public override string ToString() => $"{Amount:F2} {Currency}";
}
