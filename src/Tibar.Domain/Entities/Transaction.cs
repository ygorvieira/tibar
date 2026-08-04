using Tibar.Domain.Enums;
using Tibar.Domain.ValueObjects;

namespace Tibar.Domain.Entities;

public class Transaction : BaseEntity
{
    public string Description { get; private set; } = null!;
    public Money Amount { get; private set; } = null!;
    public TransactionType Type { get; private set; }
    public Guid CategoryId { get; private set; }
    public Category Category { get; private set; } = null!;
    public Guid AccountId { get; private set; }
    public Account Account { get; private set; } = null!;
    public Guid UserId { get; private set; }
    public DateOnly Date { get; private set; }
    public Guid? InstallmentId { get; private set; }

    private Transaction() { }

    public Transaction(
        string description,
        Money amount,
        TransactionType type,
        Guid categoryId,
        Guid accountId,
        Guid userId,
        DateOnly date,
        Guid? installmentId = null)
    {
        Description = description;
        Amount = amount;
        Type = type;
        CategoryId = categoryId;
        AccountId = accountId;
        UserId = userId;
        Date = date;
        InstallmentId = installmentId;
    }

    public void UpdateDescription(string description)
    {
        Description = description;
        MarkAsUpdated();
    }

    public void UpdateAmount(Money amount)
    {
        Amount = amount;
        MarkAsUpdated();
    }

    public void UpdateDate(DateOnly date)
    {
        Date = date;
        MarkAsUpdated();
    }

    public void UpdateCategory(Guid categoryId)
    {
        CategoryId = categoryId;
        MarkAsUpdated();
    }

    public void UpdateAccount(Guid accountId)
    {
        AccountId = accountId;
        MarkAsUpdated();
    }
}
