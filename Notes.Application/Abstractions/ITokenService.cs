namespace Notes.Application.Abstractions;

public interface ITokenService
{
    string CreateAccessToken(int userId);
}
