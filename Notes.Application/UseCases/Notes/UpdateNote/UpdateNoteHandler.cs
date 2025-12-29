using Notes.Application.Abstractions;
using Notes.Application.Contracts;
using Notes.Application.Dtos;
using Notes.Application.Errors;
using Notes.Application.UseCases.Mapping;
using Notes.Application.UseCases.Validation;

namespace Notes.Application.UseCases.Notes.UpdateNote;

public sealed class UpdateNoteHandler
{
    private readonly INoteRepository noteRepository;
    private readonly ICurrentUser currentUser;
    private readonly IClock clock;

    public UpdateNoteHandler(INoteRepository noteRepository, ICurrentUser currentUser, IClock clock)
    {
        this.noteRepository = noteRepository;
        this.currentUser = currentUser;
        this.clock = clock;
    }

    public async Task<NoteDto> Handle(UpdateNoteCommand command, CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated)
            throw new UnauthorizedException();

        if (command.NoteId <= 0)
            throw new ValidationException("NoteId must be positive");

        var title = command.Title is null ? null : Ensure.NotBlank(command.Title, nameof(command.Title));
        var content = command.Content is null ? null : Ensure.NotBlank(command.Content, nameof(command.Content));

        if (title is null && content is null && command.IsArchived is null)
            throw new ValidationException("At least one field must be provided");

        var note = await noteRepository.GetByIdAsync(command.NoteId, currentUser.UserId, ct);
        if (note is null)
            throw new NotFoundException("Note not found");

        var nowUnix = clock.UtcNow.ToUnixTimeSeconds();
        note.Update(title, content, command.IsArchived, nowUnix);

        await noteRepository.SaveChangesAsync(ct);
        return note.ToDto();
    }
}
