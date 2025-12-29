using Notes.Application.Dtos;
using Notes.Domain.Notes;
using Notes.Domain.Users;

namespace Notes.Application.UseCases.Mapping;

public static class DtoMapper
{
    public static UserDto ToDto(this User user) =>
        new(user.Id, user.Email, user.DisplayName, user.CreatedAtUtc, user.UpdatedAtUtc);

    public static NoteDto ToDto(this Note note) =>
        new(note.Id, note.Title, note.Content, note.IsArchived, note.CreatedAtUnix, note.UpdatedAtUnix);

}
