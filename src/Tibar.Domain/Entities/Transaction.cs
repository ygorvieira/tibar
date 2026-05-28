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
    public Guid UserId { get; private set; }
    public DateOnly Date { get; private set; }

    private Transaction() { }

    public Transaction(
        string description,
        Money amount,
        TransactionType type,
        Guid categoryId,
        Guid userId,
        DateOnly date)
    {
        Description = description;
        Amount = amount;
        Type = type;
        CategoryId = categoryId;
        UserId = userId;
        Date = date;
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
}
