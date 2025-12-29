namespace Notes.Application.UseCases.Notes.UpdateNote;

public sealed record UpdateNoteCommand(
    int NoteId,
    string? Title,
    string? Content,
    bool? IsArchived);
