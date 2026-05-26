namespace Tibar.Application.DTOs;

public record BalanceDto(
    decimal TotalIncome,
    decimal TotalExpense,
    decimal Balance,
    string Currency,
    DateOnly StartDate,
    DateOnly EndDate);
