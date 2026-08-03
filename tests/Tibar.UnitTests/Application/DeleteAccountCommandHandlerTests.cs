using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Tibar.Application.Commands.Accounts.Delete;
using Tibar.Application.Interfaces;
using Tibar.Domain.Entities;
using Tibar.Domain.Enums;
using Xunit;

namespace Tibar.UnitTests.Application;

public class DeleteAccountCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly DeleteAccountCommandHandler _handler;

    public DeleteAccountCommandHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _handler = new DeleteAccountCommandHandler(_contextMock.Object);
    }

    private Mock<Microsoft.EntityFrameworkCore.DbSet<Transaction>> CreateMockDbSet(List<Transaction> data)
    {
        var queryable = data.AsQueryable();
        var mock = new Mock<Microsoft.EntityFrameworkCore.DbSet<Transaction>>();
        mock.As<IQueryable<Transaction>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<Transaction>(queryable.Provider));
        mock.As<IQueryable<Transaction>>().Setup(m => m.Expression).Returns(queryable.Expression);
        mock.As<IQueryable<Transaction>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
        mock.As<IQueryable<Transaction>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());
        return mock;
    }

    private class TestAsyncQueryProvider<T> : IQueryProvider, IAsyncQueryProvider
    {
        private readonly IQueryProvider _inner;
        public TestAsyncQueryProvider(IQueryProvider inner) => _inner = inner;

        public IQueryable CreateQuery(Expression expression) => new TestAsyncEnumerable<T>(expression);
        public IQueryable<TElement> CreateQuery<TElement>(Expression expression) => new TestAsyncEnumerable<TElement>(expression);
        public object? Execute(Expression expression) => _inner.Execute(expression);
        public TResult Execute<TResult>(Expression expression) => _inner.Execute<TResult>(expression);
        public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
        {
            var expectedResultType = typeof(TResult).GetGenericArguments()[0];
            var executionResult = typeof(IQueryProvider)
                .GetMethod(nameof(IQueryProvider.Execute), 1, new[] { typeof(Expression) })!
                .MakeGenericMethod(expectedResultType)
                .Invoke(this, new[] { expression });

            return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))!
                .MakeGenericMethod(expectedResultType)
                .Invoke(null, new[] { executionResult })!;
        }
    }

    private class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
    {
        private readonly IQueryProvider _provider;
        public TestAsyncEnumerable(Expression expression) : base(expression)
        {
            _provider = new TestAsyncQueryProvider<T>(this.ToList().AsQueryable().Provider);
        }
        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            => new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
        IQueryProvider IQueryable.Provider => _provider;
    }

    private class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _inner;
        public TestAsyncEnumerator(IEnumerator<T> inner) => _inner = inner;
        public T Current => _inner.Current;
        public ValueTask<bool> MoveNextAsync() => new(_inner.MoveNext());
        public ValueTask DisposeAsync() { _inner.Dispose(); return default; }
    }

    [Fact]
    public async Task Handle_WithoutTransactions_ReturnsSuccess()
    {
        var accountId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var account = new Account("Bradesco", AccountType.Checking, userId);
        typeof(BaseEntity).GetProperty("Id")!.SetValue(account, accountId);

        var mockAccounts = new Mock<Microsoft.EntityFrameworkCore.DbSet<Account>>();
        mockAccounts.Setup(m => m.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        _contextMock.Setup(x => x.Accounts).Returns(mockAccounts.Object);
        _contextMock.Setup(x => x.Transactions).Returns(CreateMockDbSet(new List<Transaction>()).Object);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _handler.Handle(new DeleteAccountCommand(accountId, userId), CancellationToken.None);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Handle_WithTransactions_ReturnsFailure()
    {
        var accountId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var account = new Account("Bradesco", AccountType.Checking, userId);
        typeof(BaseEntity).GetProperty("Id")!.SetValue(account, accountId);

        var transactions = new List<Transaction>
        {
            new("Test", Tibar.Domain.ValueObjects.Money.Create(100), TransactionType.Expense,
                Guid.NewGuid(), accountId, userId, new DateOnly(2026, 5, 27))
        };

        var mockAccounts = new Mock<Microsoft.EntityFrameworkCore.DbSet<Account>>();
        mockAccounts.Setup(m => m.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        _contextMock.Setup(x => x.Accounts).Returns(mockAccounts.Object);
        _contextMock.Setup(x => x.Transactions).Returns(CreateMockDbSet(transactions).Object);

        var result = await _handler.Handle(new DeleteAccountCommand(accountId, userId), CancellationToken.None);

        Assert.False(result.IsValid);
        Assert.Equal("Conta possui transações vinculadas e não pode ser excluída.", result.Errors[0]);
    }

    [Fact]
    public async Task Handle_NonExistentAccount_ReturnsFailure()
    {
        var mockAccounts = new Mock<Microsoft.EntityFrameworkCore.DbSet<Account>>();
        mockAccounts.Setup(m => m.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Account?)null);

        _contextMock.Setup(x => x.Accounts).Returns(mockAccounts.Object);

        var result = await _handler.Handle(new DeleteAccountCommand(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        Assert.False(result.IsValid);
        Assert.Equal("Conta não encontrada.", result.Errors[0]);
    }
}
