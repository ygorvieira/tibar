using System.Linq.Expressions;
using Moq;
using Tibar.Application.DTOs;
using Tibar.Application.Interfaces;
using Tibar.Application.Queries.Accounts;
using Tibar.Domain.Entities;
using Tibar.Domain.Enums;
using Xunit;

namespace Tibar.UnitTests.Application;

public class GetAccountsQueryHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly GetAccountsQueryHandler _handler;

    public GetAccountsQueryHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _handler = new GetAccountsQueryHandler(_contextMock.Object);
    }

    private Mock<Microsoft.EntityFrameworkCore.DbSet<Account>> CreateMockDbSet(List<Account> data)
    {
        var queryable = data.AsQueryable();
        var mock = new Mock<Microsoft.EntityFrameworkCore.DbSet<Account>>();
        mock.As<IQueryable<Account>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<Account>(queryable.Provider));
        mock.As<IQueryable<Account>>().Setup(m => m.Expression).Returns(queryable.Expression);
        mock.As<IQueryable<Account>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
        mock.As<IQueryable<Account>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());
        return mock;
    }

    private class TestAsyncQueryProvider<T> : IQueryProvider, Microsoft.EntityFrameworkCore.Query.IAsyncQueryProvider
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
    public async Task Handle_ReturnsOnlyUserAccounts()
    {
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        var accounts = new List<Account>
        {
            new("Bradesco", AccountType.Checking, userId),
            new("Nu", AccountType.Investment, userId),
            new("Outro", AccountType.CreditCard, otherUserId)
        };

        _contextMock.Setup(x => x.Accounts).Returns(CreateMockDbSet(accounts).Object);

        var result = await _handler.Handle(new GetAccountsQuery(userId, 1, 50), CancellationToken.None);

        Assert.True(result.IsValid);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.TotalCount);
        var list = result.Data.Items.ToList();
        Assert.Equal(2, list.Count);
        Assert.Contains(list, a => a.Description == "Bradesco");
        Assert.Contains(list, a => a.Description == "Nu");
    }
}
