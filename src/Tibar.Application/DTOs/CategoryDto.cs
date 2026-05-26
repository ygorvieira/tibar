namespace Tibar.Application.DTOs;

public record CategoryDto(
    Guid Id,
    string Name,
    string Type,
    DateTime CreatedAt);
