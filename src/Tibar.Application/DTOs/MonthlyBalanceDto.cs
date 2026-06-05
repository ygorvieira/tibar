namespace Tibar.Application.DTOs;

public record MonthlyBalanceDto(
    int Year,
    int Month,
    decimal TotalIncome,
    decimal TotalExpense,
    decimal Balance,
    string Currency);
