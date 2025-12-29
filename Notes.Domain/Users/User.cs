using Notes.Domain.Common;

namespace Notes.Domain.Users;

public sealed class User : Entity
{
    private const int DisplayNameMaxLength = 50;
    private const int EmailMaxLength = 254;

    private User() { } // for ORM

    public User(string email, string passwordHash, string displayName, DateTimeOffset createdAtUtc)
    {
        Email = NormalizeEmail(email);
        PasswordHash = Guard.NotBlank(passwordHash, nameof(passwordHash));
        DisplayName = Guard.MaxLength(Guard.NotBlank(displayName, nameof(displayName)), DisplayNameMaxLength, nameof(displayName));

        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;
    }

    public string Email { get; private set; } = default!;
    public string PasswordHash { get; private set; } = default!;
    public string DisplayName { get; private set; } = default!;
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public void UpdateDisplayName(string displayName, DateTimeOffset nowUtc)
    {
        DisplayName = Guard.MaxLength(Guard.NotBlank(displayName, nameof(displayName)), DisplayNameMaxLength, nameof(displayName));
        UpdatedAtUtc = nowUtc;
    }

    public void ChangePasswordHash(string passwordHash, DateTimeOffset nowUtc)
    {
        PasswordHash = Guard.NotBlank(passwordHash, nameof(passwordHash));
        UpdatedAtUtc = nowUtc;
    }

    private static string NormalizeEmail(string email)
    {
        email = Guard.NotBlank(email, nameof(email)).ToLowerInvariant();
        email = Guard.MaxLength(email, EmailMaxLength, nameof(email));
        return email;
    }
}
