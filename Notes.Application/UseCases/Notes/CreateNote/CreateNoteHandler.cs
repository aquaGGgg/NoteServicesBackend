using Notes.Application.Abstractions;
using Notes.Application.Contracts;
using Notes.Application.Dtos;
using Notes.Application.Errors;
using Notes.Application.UseCases.Mapping;
using Notes.Application.UseCases.Validation;
using Notes.Domain.Notes;

namespace Notes.Application.UseCases.Notes.CreateNote;

public sealed class CreateNoteHandler
{
    private readonly INoteRepository noteRepository;
    private readonly ICurrentUser currentUser;
    private readonly IClock clock;

    public CreateNoteHandler(INoteRepository noteRepository, ICurrentUser currentUser, IClock clock)
    {
        this.noteRepository = noteRepository;
        this.currentUser = currentUser;
        this.clock = clock;
    }

    public async Task<NoteDto> Handle(CreateNoteCommand command, CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated)
            throw new UnauthorizedException();

        var title = Ensure.NotBlank(command.Title, nameof(command.Title));
        var content = Ensure.NotBlank(command.Content, nameof(command.Content));

        var nowUnix = clock.UtcNow.ToUnixTimeSeconds();
        var note = new Note(currentUser.UserId, title, content, nowUnix);


        await noteRepository.AddAsync(note, ct);
        await noteRepository.SaveChangesAsync(ct);

        return note.ToDto();
    }
}
