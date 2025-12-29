namespace Notes.Application.UseCases.Notes.ListNotes;

public sealed record ListNotesQuery(
    int Limit = 20,
    int Offset = 0,
    bool? Archived = null,
    string? Query = null);
