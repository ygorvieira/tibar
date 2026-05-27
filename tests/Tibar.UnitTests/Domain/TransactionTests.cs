using Tibar.Domain.Entities;
using Tibar.Domain.Enums;
using Tibar.Domain.ValueObjects;
using Xunit;

namespace Tibar.UnitTests.Domain;

public class TransactionTests
{
    private readonly Guid _categoryId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();

    private Transaction CreateValidTransaction()
    {
        var amount = Money.Create(100, "BRL");
        return new Transaction("Test", amount, TransactionType.Expense, _categoryId, _userId, new DateOnly(2026, 5, 27));
    }

    [Fact]
    public void Constructor_SetsProperties()
    {
        var date = new DateOnly(2026, 5, 27);
        var tx = CreateValidTransaction();

        Assert.Equal("Test", tx.Description);
        Assert.Equal(100, tx.Amount.Amount);
        Assert.Equal(TransactionType.Expense, tx.Type);
        Assert.Equal(_categoryId, tx.CategoryId);
        Assert.Equal(_userId, tx.UserId);
        Assert.Equal(date, tx.Date);
        Assert.NotEqual(Guid.Empty, tx.Id);
        Assert.True(tx.CreatedAt > DateTime.MinValue);
    }

    [Fact]
    public void UpdateDescription_ChangesDescription()
    {
        var tx = CreateValidTransaction();

        tx.UpdateDescription("Updated");

        Assert.Equal("Updated", tx.Description);
        Assert.NotNull(tx.UpdatedAt);
    }

    [Fact]
    public void UpdateAmount_ChangesAmount()
    {
        var tx = CreateValidTransaction();
        var newAmount = Money.Create(200, "BRL");

        tx.UpdateAmount(newAmount);

        Assert.Equal(200, tx.Amount.Amount);
        Assert.NotNull(tx.UpdatedAt);
    }

    [Fact]
    public void UpdateDate_ChangesDate()
    {
        var tx = CreateValidTransaction();
        var newDate = new DateOnly(2026, 6, 1);

        tx.UpdateDate(newDate);

        Assert.Equal(newDate, tx.Date);
        Assert.NotNull(tx.UpdatedAt);
    }

    [Fact]
    public void UpdateCategory_ChangesCategoryId()
    {
        var tx = CreateValidTransaction();
        var newCategoryId = Guid.NewGuid();

        tx.UpdateCategory(newCategoryId);

        Assert.Equal(newCategoryId, tx.CategoryId);
        Assert.NotNull(tx.UpdatedAt);
    }
}
