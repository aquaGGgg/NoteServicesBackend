using Notes.Application.Abstractions;
using Notes.Application.Contracts;
using Notes.Application.Dtos;
using Notes.Application.Errors;
using Notes.Application.UseCases.Mapping;

namespace Notes.Application.UseCases.Notes.GetNote;

public sealed class GetNoteHandler
{
    private readonly INoteRepository noteRepository;
    private readonly ICurrentUser currentUser;

    public GetNoteHandler(INoteRepository noteRepository, ICurrentUser currentUser)
    {
        this.noteRepository = noteRepository;
        this.currentUser = currentUser;
    }

    public async Task<NoteDto> Handle(GetNoteQuery query, CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated)
            throw new UnauthorizedException();

        if (query.NoteId <= 0)
            throw new ValidationException("NoteId must be positive");

        var note = await noteRepository.GetByIdAsync(query.NoteId, currentUser.UserId, ct);
        if (note is null)
            throw new NotFoundException("Note not found");

        return note.ToDto();
    }
}
