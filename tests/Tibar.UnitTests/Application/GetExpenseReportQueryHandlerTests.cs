using System.Linq.Expressions;
using Moq;
using Tibar.Application.DTOs;
using Tibar.Application.Interfaces;
using Tibar.Application.Queries.Reports;
using Tibar.Domain.Entities;
using Tibar.Domain.Enums;
using Tibar.Domain.ValueObjects;
using Xunit;

namespace Tibar.UnitTests.Application;

public class GetExpenseReportQueryHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly GetExpenseReportQueryHandler _handler;

    public GetExpenseReportQueryHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _handler = new GetExpenseReportQueryHandler(_contextMock.Object);
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
        private readonly IQueryProvider _provider;
        public TestAsyncEnumerable(Expression expression) : base(expression)
        {
            _provider = new TestAsyncQueryProvider<T>(this.AsEnumerable().AsQueryable().Provider);
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
    public async Task Handle_WithExpenses_ReturnsMonthlyReport()
    {
        var userId = Guid.NewGuid();
        var foodId = Guid.NewGuid();
        var transportId = Guid.NewGuid();
        var start = new DateOnly(2026, 5, 1);
        var end = new DateOnly(2026, 5, 31);

        var food = new Category("Alimentação", TransactionType.Expense, userId);
        typeof(BaseEntity).GetProperty("Id")!.SetValue(food, foodId);

        var transport = new Category("Transporte", TransactionType.Expense, userId);
        typeof(BaseEntity).GetProperty("Id")!.SetValue(transport, transportId);

        var transactions = new List<Transaction>
        {
            new("Supermercado", Money.Create(500), TransactionType.Expense, foodId, userId, new DateOnly(2026, 5, 5)),
            new("Restaurante", Money.Create(80), TransactionType.Expense, foodId, userId, new DateOnly(2026, 5, 10)),
            new("Supermercado", Money.Create(200), TransactionType.Expense, foodId, userId, new DateOnly(2026, 5, 15)),
            new("Uber", Money.Create(35), TransactionType.Expense, transportId, userId, new DateOnly(2026, 5, 3)),
            new("Gasolina", Money.Create(150), TransactionType.Expense, transportId, userId, new DateOnly(2026, 5, 20)),
            new("Salário", Money.Create(5000), TransactionType.Income, Guid.NewGuid(), userId, new DateOnly(2026, 5, 1)),
        };

        // Set navigation properties
        foreach (var t in transactions)
        {
            if (t.CategoryId == foodId)
                typeof(Transaction).GetProperty("Category")!.SetValue(t, food);
            else if (t.CategoryId == transportId)
                typeof(Transaction).GetProperty("Category")!.SetValue(t, transport);
        }

        var mockSet = CreateMockDbSet(transactions);
        _contextMock.Setup(x => x.Transactions).Returns(mockSet.Object);

        var query = new GetExpenseReportQuery(userId, start, end);
        var result = await _handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsValid);
        Assert.NotNull(result.Data);
        var month = Assert.Single(result.Data.Months);
        Assert.Equal(2026, month.Year);
        Assert.Equal(5, month.Month);
        Assert.Equal(2, month.TopCategories.Count);
        Assert.Equal("Alimentação", month.TopCategories[0].CategoryName);
        Assert.Equal(780, month.TopCategories[0].TotalAmount);
        Assert.Equal("Transporte", month.TopCategories[1].CategoryName);
        Assert.Equal(185, month.TopCategories[1].TotalAmount);
    }

    [Fact]
    public async Task Handle_WithMultipleMonths_ReturnsOrderedDescending()
    {
        var userId = Guid.NewGuid();
        var catId = Guid.NewGuid();
        var start = new DateOnly(2026, 4, 1);
        var end = new DateOnly(2026, 5, 31);

        var cat = new Category("Teste", TransactionType.Expense, userId);
        typeof(BaseEntity).GetProperty("Id")!.SetValue(cat, catId);

        var transactions = new List<Transaction>
        {
            new("Item", Money.Create(100), TransactionType.Expense, catId, userId, new DateOnly(2026, 4, 15)),
            new("Item", Money.Create(200), TransactionType.Expense, catId, userId, new DateOnly(2026, 5, 15)),
        };

        foreach (var t in transactions)
            typeof(Transaction).GetProperty("Category")!.SetValue(t, cat);

        var mockSet = CreateMockDbSet(transactions);
        _contextMock.Setup(x => x.Transactions).Returns(mockSet.Object);

        var query = new GetExpenseReportQuery(userId, start, end);
        var result = await _handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsValid);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Months.Count);
        Assert.Equal(2026, result.Data.Months[0].Year);
        Assert.Equal(5, result.Data.Months[0].Month);
        Assert.Equal(2026, result.Data.Months[1].Year);
        Assert.Equal(4, result.Data.Months[1].Month);
    }

    [Fact]
    public async Task Handle_WithNoExpenses_ReturnsEmpty()
    {
        var userId = Guid.NewGuid();
        var start = new DateOnly(2026, 5, 1);
        var end = new DateOnly(2026, 5, 31);

        var transactions = new List<Transaction>
        {
            new("Salary", Money.Create(5000), TransactionType.Income, Guid.NewGuid(), userId, new DateOnly(2026, 5, 5)),
        };

        var mockSet = CreateMockDbSet(transactions);
        _contextMock.Setup(x => x.Transactions).Returns(mockSet.Object);

        var query = new GetExpenseReportQuery(userId, start, end);
        var result = await _handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsValid);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data.Months);
    }

    [Fact]
    public async Task Handle_WithNoTransactions_ReturnsEmpty()
    {
        var userId = Guid.NewGuid();
        var start = new DateOnly(2026, 5, 1);
        var end = new DateOnly(2026, 5, 31);

        var transactions = new List<Transaction>();
        var mockSet = CreateMockDbSet(transactions);
        _contextMock.Setup(x => x.Transactions).Returns(mockSet.Object);

        var query = new GetExpenseReportQuery(userId, start, end);
        var result = await _handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsValid);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data.Months);
    }

    [Fact]
    public async Task Handle_Top3Categories_ReturnsOnlyThree()
    {
        var userId = Guid.NewGuid();
        var start = new DateOnly(2026, 5, 1);
        var end = new DateOnly(2026, 5, 31);

        var categories = Enumerable.Range(1, 5).Select(i =>
        {
            var c = new Category($"Cat{i}", TransactionType.Expense, userId);
            typeof(BaseEntity).GetProperty("Id")!.SetValue(c, Guid.NewGuid());
            return c;
        }).ToList();

        var transactions = categories.SelectMany<Category, Transaction>(c =>
        {
            var t = new Transaction("Item", Money.Create(100), TransactionType.Expense, c.Id, userId, new DateOnly(2026, 5, 15));
            typeof(Transaction).GetProperty("Category")!.SetValue(t, c);
            return [t];
        }).ToList();

        var mockSet = CreateMockDbSet(transactions);
        _contextMock.Setup(x => x.Transactions).Returns(mockSet.Object);

        var query = new GetExpenseReportQuery(userId, start, end);
        var result = await _handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsValid);
        var month = Assert.Single(result.Data!.Months);
        Assert.Equal(3, month.TopCategories.Count);
    }

    [Fact]
    public async Task Handle_TopDescriptions_OrdersByOccurrences()
    {
        var userId = Guid.NewGuid();
        var catId = Guid.NewGuid();
        var start = new DateOnly(2026, 5, 1);
        var end = new DateOnly(2026, 5, 31);

        var cat = new Category("Teste", TransactionType.Expense, userId);
        typeof(BaseEntity).GetProperty("Id")!.SetValue(cat, catId);

        var transactions = new List<Transaction>
        {
            new("Gasolina", Money.Create(100), TransactionType.Expense, catId, userId, new DateOnly(2026, 5, 5)),
            new("Gasolina", Money.Create(100), TransactionType.Expense, catId, userId, new DateOnly(2026, 5, 10)),
            new("Gasolina", Money.Create(100), TransactionType.Expense, catId, userId, new DateOnly(2026, 5, 15)),
            new("Uber", Money.Create(50), TransactionType.Expense, catId, userId, new DateOnly(2026, 5, 20)),
            new("Uber", Money.Create(50), TransactionType.Expense, catId, userId, new DateOnly(2026, 5, 25)),
            new("Ônibus", Money.Create(10), TransactionType.Expense, catId, userId, new DateOnly(2026, 5, 1)),
        };

        foreach (var t in transactions)
            typeof(Transaction).GetProperty("Category")!.SetValue(t, cat);

        var mockSet = CreateMockDbSet(transactions);
        _contextMock.Setup(x => x.Transactions).Returns(mockSet.Object);

        var query = new GetExpenseReportQuery(userId, start, end);
        var result = await _handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsValid);
        var category = result.Data!.Months[0].TopCategories[0];
        Assert.Equal(3, category.TopDescriptions.Count);
        Assert.Equal("Gasolina", category.TopDescriptions[0].Description);
        Assert.Equal(3, category.TopDescriptions[0].Occurrences);
        Assert.Equal("Uber", category.TopDescriptions[1].Description);
        Assert.Equal(2, category.TopDescriptions[1].Occurrences);
        Assert.Equal("Ônibus", category.TopDescriptions[2].Description);
        Assert.Equal(1, category.TopDescriptions[2].Occurrences);
    }
}
