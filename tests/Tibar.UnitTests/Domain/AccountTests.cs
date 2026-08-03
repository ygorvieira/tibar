using Tibar.Domain.Entities;
using Tibar.Domain.Enums;
using Xunit;

namespace Tibar.UnitTests.Domain;

public class AccountTests
{
    private readonly Guid _userId = Guid.NewGuid();

    [Fact]
    public void Constructor_SetsProperties()
    {
        var account = new Account("Bradesco", AccountType.Checking, _userId);

        Assert.Equal("Bradesco", account.Description);
        Assert.Equal(AccountType.Checking, account.Type);
        Assert.Equal(_userId, account.UserId);
        Assert.NotEqual(Guid.Empty, account.Id);
        Assert.True(account.CreatedAt > DateTime.MinValue);
    }

    [Fact]
    public void Update_ChangesProperties()
    {
        var account = new Account("Bradesco", AccountType.Checking, _userId);

        account.Update("Nu", AccountType.Investment);

        Assert.Equal("Nu", account.Description);
        Assert.Equal(AccountType.Investment, account.Type);
        Assert.NotNull(account.UpdatedAt);
    }
}
