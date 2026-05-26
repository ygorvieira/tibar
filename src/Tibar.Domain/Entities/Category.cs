using Tibar.Domain.Enums;

namespace Tibar.Domain.Entities;

public class Category : BaseEntity
{
    public string Name { get; private set; } = null!;
    public TransactionType Type { get; private set; }
    public Guid UserId { get; private set; }

    private Category() { }

    public Category(string name, TransactionType type, Guid userId)
    {
        Name = name;
        Type = type;
        UserId = userId;
    }

    public void Update(string name, TransactionType type)
    {
        Name = name;
        Type = type;
        MarkAsUpdated();
    }
}
