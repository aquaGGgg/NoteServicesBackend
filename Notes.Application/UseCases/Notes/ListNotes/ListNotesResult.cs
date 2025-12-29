using Notes.Application.Dtos;

namespace Notes.Application.UseCases.Notes.ListNotes;

public sealed record ListNotesResult(
    IReadOnlyList<NoteDto> Items,
    int Total,
    int Limit,
    int Offset);
