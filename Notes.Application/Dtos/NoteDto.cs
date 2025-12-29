namespace Notes.Application.Dtos;

public sealed record NoteDto(
    int Id,
    string Title,
    string Content,
    bool IsArchived,
    long CreatedAtUnix,
    long UpdatedAtUnix);
