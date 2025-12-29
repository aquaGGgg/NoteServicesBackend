namespace Notes.Application.Dtos;

public sealed record UserDto(
    int Id,
    string Email,
    string DisplayName,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);
