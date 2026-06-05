namespace Tibar.Application.DTOs;

public record DescriptionSummaryDto(
    string Description,
    int Occurrences,
    decimal TotalAmount);

public record CategoryExpenseDto(
    Guid CategoryId,
    string CategoryName,
    decimal TotalAmount,
    List<DescriptionSummaryDto> TopDescriptions);

public record MonthlyCategoryReportDto(
    int Year,
    int Month,
    List<CategoryExpenseDto> TopCategories);

public record ExpenseReportDto(
    List<MonthlyCategoryReportDto> Months);
