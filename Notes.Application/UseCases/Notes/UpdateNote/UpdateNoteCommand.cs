namespace Notes.Application.UseCases.Notes.UpdateNote;

public sealed record UpdateNoteCommand(
    int NoteId,
    int ExpectedVersion,
    string? Title,
    string? Content,
    bool? IsArchived);
