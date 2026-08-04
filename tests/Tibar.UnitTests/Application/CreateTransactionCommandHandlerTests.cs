using Moq;
using Tibar.Application.Commands.Transactions.Create;
using Tibar.Application.Interfaces;
using Tibar.Domain.Entities;
using Tibar.Domain.Enums;
using Tibar.Domain.ValueObjects;
using Xunit;

namespace Tibar.UnitTests.Application;

public class CreateTransactionCommandHandlerTests
{
    private readonly Mock<Microsoft.EntityFrameworkCore.DbSet<Transaction>> _transactionsMock;
    private readonly Mock<Microsoft.EntityFrameworkCore.DbSet<Category>> _categoriesMock;
    private readonly Mock<Microsoft.EntityFrameworkCore.DbSet<Account>> _accountsMock;
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly CreateTransactionCommandHandler _handler;
    private readonly Guid _userId = Guid.NewGuid();

    private List<Transaction> _added = [];

    public CreateTransactionCommandHandlerTests()
    {
        _transactionsMock = new Mock<Microsoft.EntityFrameworkCore.DbSet<Transaction>>();
        _transactionsMock.Setup(m => m.AddRange(It.IsAny<IEnumerable<Transaction>>()))
            .Callback<IEnumerable<Transaction>>(list => _added = list.ToList());

        _categoriesMock = new Mock<Microsoft.EntityFrameworkCore.DbSet<Category>>();
        _categoriesMock.Setup(m => m.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Category("Food", TransactionType.Expense, _userId));

        _accountsMock = new Mock<Microsoft.EntityFrameworkCore.DbSet<Account>>();
        _accountsMock.Setup(m => m.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Account("Bradesco", AccountType.Checking, _userId));

        _contextMock = new Mock<IApplicationDbContext>();
        _contextMock.Setup(x => x.Transactions).Returns(_transactionsMock.Object);
        _contextMock.Setup(x => x.Categories).Returns(_categoriesMock.Object);
        _contextMock.Setup(x => x.Accounts).Returns(_accountsMock.Object);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _handler = new CreateTransactionCommandHandler(_contextMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccessWithDto()
    {
        var command = new CreateTransactionCommand(
            "Lunch", 50, "Expense", Guid.NewGuid(), Guid.NewGuid(), _userId, new DateOnly(2026, 5, 27));

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsValid);
        Assert.NotNull(result.Data);
        var dto = Assert.Single(result.Data);
        Assert.Equal("Lunch", dto.Description);
        Assert.Equal(50, dto.Amount);
        Assert.Equal("Expense", dto.Type);
        Assert.Null(dto.InstallmentId);
    }

    [Fact]
    public async Task Handle_InvalidType_ReturnsFailure()
    {
        var command = new CreateTransactionCommand(
            "Test", 50, "InvalidType", Guid.NewGuid(), Guid.NewGuid(), _userId, new DateOnly(2026, 5, 27));

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Contains("inválido", result.Errors[0]);
    }

    [Fact]
    public async Task Handle_NonExistentCategory_ReturnsFailure()
    {
        _categoriesMock.Setup(m => m.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        var command = new CreateTransactionCommand(
            "Test", 50, "Expense", Guid.NewGuid(), Guid.NewGuid(), _userId, new DateOnly(2026, 5, 27));

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsValid);
        Assert.Equal("Categoria não encontrada.", result.Errors[0]);
    }

    [Fact]
    public async Task Handle_NonExistentAccount_ReturnsFailure()
    {
        _accountsMock.Setup(m => m.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Account?)null);

        var command = new CreateTransactionCommand(
            "Test", 50, "Expense", Guid.NewGuid(), Guid.NewGuid(), _userId, new DateOnly(2026, 5, 27));

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsValid);
        Assert.Equal("Conta não encontrada.", result.Errors[0]);
    }

    [Fact]
    public async Task Handle_NegativeAmount_ReturnsFailure()
    {
        var command = new CreateTransactionCommand(
            "Test", -10, "Expense", Guid.NewGuid(), Guid.NewGuid(), _userId, new DateOnly(2026, 5, 27));

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsValid);
        Assert.Contains("negativo", result.Errors[0].ToLower());
    }

    [Fact]
    public async Task Handle_WithInstallments_CreatesGroupWithSameAmountAndInstallmentId()
    {
        var command = new CreateTransactionCommand(
            "Laptop", 100, "Expense", Guid.NewGuid(), Guid.NewGuid(), _userId, new DateOnly(2026, 1, 1), Installments: 5);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsValid);
        Assert.NotNull(result.Data);
        Assert.Equal(5, result.Data.Count);
        Assert.Equal(5, _added.Count);
        Assert.Single(_added.Select(t => t.InstallmentId).Distinct());
        Assert.NotNull(_added[0].InstallmentId);
        Assert.All(_added, t =>
        {
            Assert.Equal(100, t.Amount.Amount);
            Assert.Equal("Laptop", t.Description);
        });
    }

    [Fact]
    public async Task Handle_WithInstallments_CreatesConsecutiveDates()
    {
        var command = new CreateTransactionCommand(
            "Laptop", 100, "Expense", Guid.NewGuid(), Guid.NewGuid(), _userId, new DateOnly(2026, 1, 1), Installments: 5);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsValid);
        Assert.NotNull(result.Data);
        Assert.Equal(5, result.Data.Count);
        Assert.Equal(
            new[] {
                new DateOnly(2026, 1, 1),
                new DateOnly(2026, 2, 1),
                new DateOnly(2026, 3, 1),
                new DateOnly(2026, 4, 1),
                new DateOnly(2026, 5, 1)
            },
            result.Data.Select(d => d.Date));
    }

    [Fact]
    public async Task Handle_WithInstallments_ClampsDateToLastDayOfMonth()
    {
        var command = new CreateTransactionCommand(
            "Laptop", 100, "Expense", Guid.NewGuid(), Guid.NewGuid(), _userId, new DateOnly(2026, 1, 31), Installments: 4);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsValid);
        Assert.NotNull(result.Data);
        Assert.Equal(
            new[] {
                new DateOnly(2026, 1, 31),
                new DateOnly(2026, 2, 28),
                new DateOnly(2026, 3, 31),
                new DateOnly(2026, 4, 30)
            },
            result.Data.Select(d => d.Date));
    }

    [Fact]
    public async Task Handle_WithInstallments_LeapYear_UsesFeb29()
    {
        var command = new CreateTransactionCommand(
            "Laptop", 100, "Expense", Guid.NewGuid(), Guid.NewGuid(), _userId, new DateOnly(2024, 1, 31), Installments: 3);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsValid);
        Assert.NotNull(result.Data);
        Assert.Equal(
            new[] {
                new DateOnly(2024, 1, 31),
                new DateOnly(2024, 2, 29),
                new DateOnly(2024, 3, 31)
            },
            result.Data.Select(d => d.Date));
    }
}
