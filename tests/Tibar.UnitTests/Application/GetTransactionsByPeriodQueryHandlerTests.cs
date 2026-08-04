using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Tibar.Application.Common;
using Tibar.Application.Interfaces;
using Tibar.Application.Queries.Transactions;
using Tibar.Domain.Entities;
using Tibar.Domain.Enums;
using Tibar.Domain.ValueObjects;
using Xunit;

namespace Tibar.UnitTests.Application;

public class GetTransactionsByPeriodQueryHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly GetTransactionsByPeriodQueryHandler _handler;

    public GetTransactionsByPeriodQueryHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _handler = new GetTransactionsByPeriodQueryHandler(_contextMock.Object);
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
    public async Task Handle_FiltersByAccount_ReturnsOnlyThatAccount()
    {
        var userId = Guid.NewGuid();
        var accountA = Guid.NewGuid();
        var accountB = Guid.NewGuid();
        var catId = Guid.NewGuid();
        var start = new DateOnly(2026, 5, 1);
        var end = new DateOnly(2026, 5, 31);

        var cat = new Category("Food", TransactionType.Expense, userId);
        typeof(BaseEntity).GetProperty("Id")!.SetValue(cat, catId);

        var transactions = new List<Transaction>
        {
            new("Item A", Money.Create(100), TransactionType.Expense, catId, accountA, userId, new DateOnly(2026, 5, 15)),
            new("Item B", Money.Create(200), TransactionType.Expense, catId, accountB, userId, new DateOnly(2026, 5, 15)),
        };

        foreach (var t in transactions)
        {
            typeof(Transaction).GetProperty("Category")!.SetValue(t, cat);
            if (t.AccountId == accountA)
                typeof(Transaction).GetProperty("Account")!.SetValue(t, new Account("Bradesco", AccountType.Checking, userId));
            else
                typeof(Transaction).GetProperty("Account")!.SetValue(t, new Account("Nu", AccountType.Investment, userId));
        }

        var mockSet = CreateMockDbSet(transactions);
        _contextMock.Setup(x => x.Transactions).Returns(mockSet.Object);

        var query = new GetTransactionsByPeriodQuery(userId, start, end, AccountId: accountA);
        var result = await _handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsValid);
        Assert.NotNull(result.Data);
        Assert.Equal(1, result.Data.TotalCount);
        var items = result.Data.Items.ToList();
        Assert.Equal("Item A", items[0].Description);
        Assert.Equal(accountA, items[0].AccountId);
        Assert.Equal("Bradesco", items[0].AccountName);
    }

    [Fact]
    public async Task Handle_WithoutAccountFilter_ReturnsAll()
    {
        var userId = Guid.NewGuid();
        var accountA = Guid.NewGuid();
        var accountB = Guid.NewGuid();
        var catId = Guid.NewGuid();
        var start = new DateOnly(2026, 5, 1);
        var end = new DateOnly(2026, 5, 31);

        var cat = new Category("Food", TransactionType.Expense, userId);
        typeof(BaseEntity).GetProperty("Id")!.SetValue(cat, catId);

        var transactions = new List<Transaction>
        {
            new("Item A", Money.Create(100), TransactionType.Expense, catId, accountA, userId, new DateOnly(2026, 5, 15)),
            new("Item B", Money.Create(200), TransactionType.Expense, catId, accountB, userId, new DateOnly(2026, 5, 15)),
        };

        foreach (var t in transactions)
        {
            typeof(Transaction).GetProperty("Category")!.SetValue(t, cat);
            typeof(Transaction).GetProperty("Account")!.SetValue(t, new Account("Bradesco", AccountType.Checking, userId));
        }

        var mockSet = CreateMockDbSet(transactions);
        _contextMock.Setup(x => x.Transactions).Returns(mockSet.Object);

        var query = new GetTransactionsByPeriodQuery(userId, start, end);
        var result = await _handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsValid);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.TotalCount);
    }
}
