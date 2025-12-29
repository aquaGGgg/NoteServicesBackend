using Notes.Application.Abstractions;
using Notes.Application.Contracts;
using Notes.Application.Errors;

namespace Notes.Application.UseCases.Notes.DeleteNote;

public sealed class DeleteNoteHandler
{
    private readonly INoteRepository noteRepository;
    private readonly ICurrentUser currentUser;

    public DeleteNoteHandler(INoteRepository noteRepository, ICurrentUser currentUser)
    {
        this.noteRepository = noteRepository;
        this.currentUser = currentUser;
    }

    public async Task Handle(DeleteNoteCommand command, CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated)
            throw new UnauthorizedException();

        if (command.NoteId <= 0)
            throw new ValidationException("NoteId must be positive");

        var note = await noteRepository.GetByIdAsync(command.NoteId, currentUser.UserId, ct);
        if (note is null)
            throw new NotFoundException("Note not found");

        await noteRepository.DeleteAsync(note, ct);
        await noteRepository.SaveChangesAsync(ct);
    }
}
