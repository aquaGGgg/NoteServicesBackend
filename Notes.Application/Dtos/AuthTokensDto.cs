namespace Notes.Application.Dtos;

public sealed record AuthTokensDto(
    string AccessToken,
    string RefreshToken);
