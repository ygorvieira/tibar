using MediatR;
using Moq;
using Tibar.Application.Commands.Transactions.Create;
using Tibar.Application.Common;
using Tibar.Application.Interfaces;
using Tibar.Domain.Entities;
using Tibar.Domain.Enums;
using Tibar.Domain.ValueObjects;
using Xunit;

namespace Tibar.UnitTests.Application;

public class CreateTransactionCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly CreateTransactionCommandHandler _handler;

    public CreateTransactionCommandHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _handler = new CreateTransactionCommandHandler(_contextMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccessWithDto()
    {
        var categoryId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var category = new Category("Food", TransactionType.Expense, userId);

        var categories = new List<Category> { category }.AsQueryable();
        var mockCategories = new Mock<Microsoft.EntityFrameworkCore.DbSet<Category>>();
        mockCategories.As<IQueryable<Category>>().Setup(m => m.Provider).Returns(categories.Provider);
        mockCategories.As<IQueryable<Category>>().Setup(m => m.Expression).Returns(categories.Expression);
        mockCategories.As<IQueryable<Category>>().Setup(m => m.ElementType).Returns(categories.ElementType);
        mockCategories.As<IQueryable<Category>>().Setup(m => m.GetEnumerator()).Returns(categories.GetEnumerator());
        mockCategories.Setup(m => m.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _contextMock.Setup(x => x.Categories).Returns(mockCategories.Object);
        _contextMock.Setup(x => x.Transactions).Returns(new Mock<Microsoft.EntityFrameworkCore.DbSet<Transaction>>().Object);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new CreateTransactionCommand(
            "Lunch", 50, "Expense", categoryId, userId, new DateOnly(2026, 5, 27));

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsValid);
        Assert.NotNull(result.Data);
        Assert.Equal("Lunch", result.Data.Description);
        Assert.Equal(50, result.Data.Amount);
        Assert.Equal("Expense", result.Data.Type);
    }

    [Fact]
    public async Task Handle_InvalidType_ReturnsFailure()
    {
        var command = new CreateTransactionCommand(
            "Test", 50, "InvalidType", Guid.NewGuid(), Guid.NewGuid(), new DateOnly(2026, 5, 27));

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Contains("Invalid", result.Errors[0]);
    }

    [Fact]
    public async Task Handle_NonExistentCategory_ReturnsFailure()
    {
        var categoryId = Guid.NewGuid();
        var mockCategories = new Mock<Microsoft.EntityFrameworkCore.DbSet<Category>>();
        mockCategories.Setup(m => m.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        _contextMock.Setup(x => x.Categories).Returns(mockCategories.Object);

        var command = new CreateTransactionCommand(
            "Test", 50, "Expense", categoryId, Guid.NewGuid(), new DateOnly(2026, 5, 27));

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsValid);
        Assert.Equal("Category not found.", result.Errors[0]);
    }

    [Fact]
    public async Task Handle_NegativeAmount_ReturnsFailure()
    {
        var categoryId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var category = new Category("Food", TransactionType.Expense, userId);

        var mockCategories = new Mock<Microsoft.EntityFrameworkCore.DbSet<Category>>();
        mockCategories.Setup(m => m.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _contextMock.Setup(x => x.Categories).Returns(mockCategories.Object);

        var command = new CreateTransactionCommand(
            "Test", -10, "Expense", categoryId, userId, new DateOnly(2026, 5, 27));

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsValid);
        Assert.Contains("negative", result.Errors[0].ToLower());
    }
}
