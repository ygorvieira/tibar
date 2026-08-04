using Moq;
using Tibar.Application.Commands.Accounts.Create;
using Tibar.Application.Interfaces;
using Tibar.Domain.Entities;
using Xunit;

namespace Tibar.UnitTests.Application;

public class CreateAccountCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly CreateAccountCommandHandler _handler;

    public CreateAccountCommandHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _handler = new CreateAccountCommandHandler(_contextMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccessWithDto()
    {
        _contextMock.Setup(x => x.Accounts).Returns(new Mock<Microsoft.EntityFrameworkCore.DbSet<Account>>().Object);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new CreateAccountCommand("Nu", "investment", Guid.NewGuid());
        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsValid);
        Assert.NotNull(result.Data);
        Assert.Equal("Nu", result.Data.Description);
        Assert.Equal("Investment", result.Data.Type);
    }

    [Fact]
    public async Task Handle_InvalidType_ReturnsFailure()
    {
        var command = new CreateAccountCommand("Nu", "invalid", Guid.NewGuid());
        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsValid);
        Assert.Contains("inválido", result.Errors[0]);
    }

    [Fact]
    public async Task Handle_ValidTypes_AreAccepted()
    {
        _contextMock.Setup(x => x.Accounts).Returns(new Mock<Microsoft.EntityFrameworkCore.DbSet<Account>>().Object);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var checking = await _handler.Handle(new CreateAccountCommand("C", "checking", Guid.NewGuid()), CancellationToken.None);
        var investment = await _handler.Handle(new CreateAccountCommand("C", "investment", Guid.NewGuid()), CancellationToken.None);
        var creditCard = await _handler.Handle(new CreateAccountCommand("C", "creditcard", Guid.NewGuid()), CancellationToken.None);

        Assert.True(checking.IsValid);
        Assert.Equal("Checking", checking.Data!.Type);
        Assert.Equal("Investment", investment.Data!.Type);
        Assert.Equal("CreditCard", creditCard.Data!.Type);
    }
}
