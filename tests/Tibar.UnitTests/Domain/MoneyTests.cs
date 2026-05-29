using Tibar.Domain.Exceptions;
using Tibar.Domain.ValueObjects;
using Xunit;

namespace Tibar.UnitTests.Domain;

public class MoneyTests
{
    [Fact]
    public void Create_WithValidAmount_ReturnsMoney()
    {
        var money = Money.Create(100.50m);

        Assert.Equal(100.50m, money.Amount);
        Assert.Equal("BRL", money.Currency);
    }

    [Fact]
    public void Create_WithCustomCurrency_UsesUpperCase()
    {
        var money = Money.Create(50, "usd");

        Assert.Equal("USD", money.Currency);
    }

    [Fact]
    public void Create_WithNegativeAmount_ThrowsDomainException()
    {
        var ex = Assert.Throws<DomainException>(() => Money.Create(-1));
        Assert.Equal("O valor não pode ser negativo", ex.Message);
    }

    [Fact]
    public void Create_WithZeroAmount_ReturnsMoney()
    {
        var money = Money.Create(0);

        Assert.Equal(0, money.Amount);
    }

    [Fact]
    public void Create_WithEmptyCurrency_ThrowsDomainException()
    {
        var ex = Assert.Throws<DomainException>(() => Money.Create(100, ""));
        Assert.Equal("Moeda é obrigatória", ex.Message);
    }

    [Fact]
    public void Create_WithNullCurrency_ThrowsDomainException()
    {
        var ex = Assert.Throws<DomainException>(() => Money.Create(100, null!));
        Assert.Equal("Moeda é obrigatória", ex.Message);
    }

    [Fact]
    public void Addition_SameCurrency_ReturnsSum()
    {
        var a = Money.Create(100, "BRL");
        var b = Money.Create(50, "BRL");

        var result = a + b;

        Assert.Equal(150, result.Amount);
        Assert.Equal("BRL", result.Currency);
    }

    [Fact]
    public void Addition_DifferentCurrencies_ThrowsDomainException()
    {
        var a = Money.Create(100, "BRL");
        var b = Money.Create(50, "USD");

        var ex = Assert.Throws<DomainException>(() => a + b);
        Assert.Equal("Não é possível somar moedas diferentes", ex.Message);
    }

    [Fact]
    public void Subtraction_SameCurrency_ReturnsDifference()
    {
        var a = Money.Create(100, "BRL");
        var b = Money.Create(30, "BRL");

        var result = a - b;

        Assert.Equal(70, result.Amount);
        Assert.Equal("BRL", result.Currency);
    }

    [Fact]
    public void Subtraction_DifferentCurrencies_ThrowsDomainException()
    {
        var a = Money.Create(100, "BRL");
        var b = Money.Create(30, "USD");

        var ex = Assert.Throws<DomainException>(() => a - b);
        Assert.Equal("Não é possível subtrair moedas diferentes", ex.Message);
    }

    [Fact]
    public void ToString_ReturnsFormattedString()
    {
        var money = Money.Create(100.50m);

        Assert.Equal("100,50 BRL", money.ToString());
    }

    [Fact]
    public void Equality_SameValueAndCurrency_AreEqual()
    {
        var a = Money.Create(100, "BRL");
        var b = Money.Create(100, "BRL");

        Assert.Equal(a, b);
        Assert.True(a == b);
    }

    [Fact]
    public void Equality_DifferentAmount_AreNotEqual()
    {
        var a = Money.Create(100, "BRL");
        var b = Money.Create(200, "BRL");

        Assert.NotEqual(a, b);
        Assert.True(a != b);
    }
}
