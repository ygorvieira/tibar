using Tibar.Domain.Enums;

namespace Tibar.Domain.Entities;

public class Account : BaseEntity
{
    public string Description { get; private set; } = null!;
    public AccountType Type { get; private set; }
    public Guid UserId { get; private set; }

    private Account() { }

    public Account(string description, AccountType type, Guid userId)
    {
        Description = description;
        Type = type;
        UserId = userId;
    }

    public void Update(string description, AccountType type)
    {
        Description = description;
        Type = type;
        MarkAsUpdated();
    }
}
