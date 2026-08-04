using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Tibar.Application.Commands.Transactions.Delete;
using Tibar.Application.Interfaces;
using Tibar.Domain.Entities;
using Tibar.Domain.Enums;
using Tibar.Domain.ValueObjects;
using Xunit;

namespace Tibar.UnitTests.Application;

public class DeleteTransactionCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly DeleteTransactionCommandHandler _handler;

    public DeleteTransactionCommandHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _handler = new DeleteTransactionCommandHandler(_contextMock.Object);
    }

    private static Mock<Microsoft.EntityFrameworkCore.DbSet<Transaction>> CreateMockDbSet(List<Transaction> data)
    {
        var queryable = data.AsQueryable();
        var mock = new Mock<Microsoft.EntityFrameworkCore.DbSet<Transaction>>();
        mock.As<IQueryable<Transaction>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<Transaction>(queryable.Provider));
        mock.As<IQueryable<Transaction>>().Setup(m => m.Expression).Returns(queryable.Expression);
        mock.As<IQueryable<Transaction>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
        mock.As<IQueryable<Transaction>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());
        return mock;
    }

    [Fact]
    public async Task Handle_NotInstallment_RemovesSingleTransaction()
    {
        var userId = Guid.NewGuid();
        var target = new Transaction("Old", Money.Create(10), TransactionType.Expense, Guid.NewGuid(), Guid.NewGuid(), userId, new DateOnly(2026, 1, 1));

        var mockTransactions = CreateMockDbSet(new List<Transaction> { target });
        mockTransactions.Setup(m => m.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(target);
        mockTransactions.Setup(m => m.Remove(It.IsAny<Transaction>()));

        _contextMock.Setup(x => x.Transactions).Returns(mockTransactions.Object);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _handler.Handle(new DeleteTransactionCommand(target.Id, userId), CancellationToken.None);

        Assert.True(result.IsValid);
        mockTransactions.Verify(m => m.Remove(target), Times.Once);
        mockTransactions.Verify(m => m.RemoveRange(It.IsAny<IEnumerable<Transaction>>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithInstallments_RemovesWholeGroup()
    {
        var userId = Guid.NewGuid();
        var installmentId = Guid.NewGuid();
        var target = new Transaction("Old", Money.Create(10), TransactionType.Expense, Guid.NewGuid(), Guid.NewGuid(), userId, new DateOnly(2026, 1, 1), installmentId);
        var member = new Transaction("Old", Money.Create(10), TransactionType.Expense, Guid.NewGuid(), Guid.NewGuid(), userId, new DateOnly(2026, 2, 1), installmentId);

        var mockTransactions = CreateMockDbSet(new List<Transaction> { target, member });
        mockTransactions.Setup(m => m.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(target);
        mockTransactions.Setup(m => m.RemoveRange(It.IsAny<IEnumerable<Transaction>>()));

        _contextMock.Setup(x => x.Transactions).Returns(mockTransactions.Object);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _handler.Handle(new DeleteTransactionCommand(target.Id, userId), CancellationToken.None);

        Assert.True(result.IsValid);
        mockTransactions.Verify(m => m.Remove(It.IsAny<Transaction>()), Times.Never);
        mockTransactions.Verify(m => m.RemoveRange(It.Is<IEnumerable<Transaction>>(g => g.Count() == 2)), Times.Once);
    }

    [Fact]
    public async Task Handle_NonExistentTransaction_ReturnsFailure()
    {
        var mockTransactions = new Mock<Microsoft.EntityFrameworkCore.DbSet<Transaction>>();
        mockTransactions.Setup(m => m.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Transaction?)null);

        _contextMock.Setup(x => x.Transactions).Returns(mockTransactions.Object);

        var result = await _handler.Handle(new DeleteTransactionCommand(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        Assert.False(result.IsValid);
        Assert.Equal("Transação não encontrada.", result.Errors[0]);
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
}
