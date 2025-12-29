using Notes.Domain.Common;

namespace Notes.Domain.Notes;

public sealed class Note : Entity
{
    private const int TitleMaxLength = 200;

    private Note() { } // for ORM

    public Note(int userId, string title, string content, long createdAtUnix)
    {
        UserId = Guard.Positive(userId, nameof(userId));
        Title = NormalizeTitle(title);
        Content = NormalizeContent(content);

        IsArchived = false;
        CreatedAtUnix = createdAtUnix;
        UpdatedAtUnix = createdAtUnix;
    }

    public int UserId { get; private set; }
    public string Title { get; private set; } = default!;
    public string Content { get; private set; } = default!;
    public bool IsArchived { get; private set; }

    public long CreatedAtUnix { get; private set; }
    public long UpdatedAtUnix { get; private set; }

    public void Update(string? title, string? content, bool? isArchived, long nowUnix)
    {
        if (title is not null)
            Title = NormalizeTitle(title);

        if (content is not null)
            Content = NormalizeContent(content);

        if (isArchived.HasValue)
            IsArchived = isArchived.Value;

        UpdatedAtUnix = nowUnix;
    }

    private static string NormalizeTitle(string title)
    {
        title = Guard.NotBlank(title, nameof(title));
        title = Guard.MaxLength(title, TitleMaxLength, nameof(title));
        return title;
    }

    private static string NormalizeContent(string content)
    {
        return Guard.NotBlank(content, nameof(content));
    }
}
