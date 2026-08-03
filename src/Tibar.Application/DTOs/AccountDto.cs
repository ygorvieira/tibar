namespace Tibar.Application.DTOs;

public record AccountDto(
    Guid Id,
    string Description,
    string Type,
    DateTime CreatedAt);
