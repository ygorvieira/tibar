using System.Collections;
using System.Linq.Expressions;
using Moq;
using Tibar.Application.Interfaces;
using Tibar.Application.Queries.Dashboard;
using Tibar.Domain.Entities;
using Tibar.Domain.Enums;
using Tibar.Domain.ValueObjects;
using Xunit;

namespace Tibar.UnitTests.Application;

public class GetBalanceByPeriodQueryHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly GetBalanceByPeriodQueryHandler _handler;

    public GetBalanceByPeriodQueryHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _handler = new GetBalanceByPeriodQueryHandler(_contextMock.Object);
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

    private class TestAsyncQueryProvider<T> : IQueryProvider
    {
        private readonly IQueryProvider _inner;
        public TestAsyncQueryProvider(IQueryProvider inner) => _inner = inner;

        public IQueryable CreateQuery(Expression expression) => new TestAsyncEnumerable<T>(expression);
        public IQueryable<TElement> CreateQuery<TElement>(Expression expression) => new TestAsyncEnumerable<TElement>(expression);
        public object? Execute(Expression expression) => _inner.Execute(expression);
        public TResult Execute<TResult>(Expression expression) => _inner.Execute<TResult>(expression);
    }

    private class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
    {
        public TestAsyncEnumerable(Expression expression) : base(expression) { }
        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            => new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
        IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this.AsQueryable().Provider);
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
    public async Task Handle_WithTransactions_ReturnsCorrectBalance()
    {
        var userId = Guid.NewGuid();
        var start = new DateOnly(2026, 5, 1);
        var end = new DateOnly(2026, 5, 31);

        var transactions = new List<Transaction>
        {
            new("Salary", Money.Create(5000), TransactionType.Income, Guid.NewGuid(), userId, new DateOnly(2026, 5, 5)),
            new("Rent", Money.Create(1500), TransactionType.Expense, Guid.NewGuid(), userId, new DateOnly(2026, 5, 5)),
            new("Food", Money.Create(300), TransactionType.Expense, Guid.NewGuid(), userId, new DateOnly(2026, 5, 10)),
            new("Freelance", Money.Create(1000), TransactionType.Income, Guid.NewGuid(), userId, new DateOnly(2026, 5, 15)),
        };

        var mockSet = CreateMockDbSet(transactions);
        _contextMock.Setup(x => x.Transactions).Returns(mockSet.Object);

        var query = new GetBalanceByPeriodQuery(userId, start, end);
        var result = await _handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsValid);
        Assert.NotNull(result.Data);
        Assert.Equal(6000, result.Data.TotalIncome);
        Assert.Equal(1800, result.Data.TotalExpense);
        Assert.Equal(4200, result.Data.Balance);
        Assert.Equal("BRL", result.Data.Currency);
    }

    [Fact]
    public async Task Handle_WithNoTransactions_ReturnsZeroBalance()
    {
        var userId = Guid.NewGuid();
        var start = new DateOnly(2026, 5, 1);
        var end = new DateOnly(2026, 5, 31);

        var mockSet = CreateMockDbSet(new List<Transaction>());
        _contextMock.Setup(x => x.Transactions).Returns(mockSet.Object);

        var query = new GetBalanceByPeriodQuery(userId, start, end);
        var result = await _handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsValid);
        Assert.Equal(0, result.Data.TotalIncome);
        Assert.Equal(0, result.Data.TotalExpense);
        Assert.Equal(0, result.Data.Balance);
        Assert.Equal("BRL", result.Data.Currency);
    }

    [Fact]
    public async Task Handle_FiltersByUserId_ExcludesOtherUsers()
    {
        var userA = Guid.NewGuid();
        var userB = Guid.NewGuid();
        var start = new DateOnly(2026, 5, 1);
        var end = new DateOnly(2026, 5, 31);

        var transactions = new List<Transaction>
        {
            new("Salary", Money.Create(5000), TransactionType.Income, Guid.NewGuid(), userA, new DateOnly(2026, 5, 5)),
            new("Salary", Money.Create(3000), TransactionType.Income, Guid.NewGuid(), userB, new DateOnly(2026, 5, 5)),
        };

        var mockSet = CreateMockDbSet(transactions);
        _contextMock.Setup(x => x.Transactions).Returns(mockSet.Object);

        var query = new GetBalanceByPeriodQuery(userA, start, end);
        var result = await _handler.Handle(query, CancellationToken.None);

        Assert.Equal(5000, result.Data.TotalIncome);
    }

    [Fact]
    public async Task Handle_FiltersByPeriod_ExcludesOutsideRange()
    {
        var userId = Guid.NewGuid();
        var start = new DateOnly(2026, 5, 1);
        var end = new DateOnly(2026, 5, 31);

        var transactions = new List<Transaction>
        {
            new("In", Money.Create(100), TransactionType.Income, Guid.NewGuid(), userId, new DateOnly(2026, 5, 15)),
            new("Outside", Money.Create(100), TransactionType.Income, Guid.NewGuid(), userId, new DateOnly(2026, 6, 15)),
        };

        var mockSet = CreateMockDbSet(transactions);
        _contextMock.Setup(x => x.Transactions).Returns(mockSet.Object);

        var query = new GetBalanceByPeriodQuery(userId, start, end);
        var result = await _handler.Handle(query, CancellationToken.None);

        Assert.Equal(100, result.Data.TotalIncome);
    }
}
