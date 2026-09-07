using Tibar.Application.Queries.Accounts;
using Tibar.Application.Queries.Categories;
using Tibar.Application.Queries.Transactions;
using Xunit;

namespace Tibar.UnitTests.Application;

public class PaginationQueryValidatorTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Transactions_InvalidPage_Fails(int page)
    {
        var validator = new GetTransactionsByPeriodQueryValidator();
        var result = validator.Validate(new GetTransactionsByPeriodQuery(Guid.NewGuid(), new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31), page));
        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public void Transactions_InvalidPageSize_Fails(int pageSize)
    {
        var validator = new GetTransactionsByPeriodQueryValidator();
        var result = validator.Validate(new GetTransactionsByPeriodQuery(Guid.NewGuid(), new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31), PageSize: pageSize));
        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Categories_InvalidPage_Fails(int page)
    {
        var validator = new GetCategoriesQueryValidator();
        var result = validator.Validate(new GetCategoriesQuery(Guid.NewGuid(), page));
        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public void Categories_InvalidPageSize_Fails(int pageSize)
    {
        var validator = new GetCategoriesQueryValidator();
        var result = validator.Validate(new GetCategoriesQuery(Guid.NewGuid(), PageSize: pageSize));
        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Accounts_InvalidPage_Fails(int page)
    {
        var validator = new GetAccountsQueryValidator();
        var result = validator.Validate(new GetAccountsQuery(Guid.NewGuid(), page));
        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public void Accounts_InvalidPageSize_Fails(int pageSize)
    {
        var validator = new GetAccountsQueryValidator();
        var result = validator.Validate(new GetAccountsQuery(Guid.NewGuid(), PageSize: pageSize));
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Transactions_ValidPagination_Passes()
    {
        var validator = new GetTransactionsByPeriodQueryValidator();
        var result = validator.Validate(new GetTransactionsByPeriodQuery(Guid.NewGuid(), new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31), 1, 50));
        Assert.True(result.IsValid);
    }
}
