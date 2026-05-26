namespace Tibar.Application.DTOs;

public record TransactionDto(
    Guid Id,
    string Description,
    decimal Amount,
    string Currency,
    string Type,
    Guid CategoryId,
    string CategoryName,
    DateOnly Date,
    DateTime CreatedAt);
