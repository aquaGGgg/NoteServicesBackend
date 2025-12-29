namespace Notes.Infrastructure.Persistence.Models;

public sealed class UserToken
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string TokenHash { get; set; } = "";
    public long CreatedAtUnix { get; set; }
    public long ExpiresAtUnix { get; set; }
    public long? RevokedAtUnix { get; set; }
}
