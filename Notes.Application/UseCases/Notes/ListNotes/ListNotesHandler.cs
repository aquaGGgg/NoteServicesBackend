using Notes.Application.Abstractions;
using Notes.Application.Contracts;
using Notes.Application.Errors;
using Notes.Application.UseCases.Mapping;
using Notes.Application.UseCases.Validation;

namespace Notes.Application.UseCases.Notes.ListNotes;

public sealed class ListNotesHandler
{
    private const int DefaultLimit = 20;
    private const int MaxLimit = 100;

    private readonly INoteRepository noteRepository;
    private readonly ICurrentUser currentUser;

    public ListNotesHandler(INoteRepository noteRepository, ICurrentUser currentUser)
    {
        this.noteRepository = noteRepository;
        this.currentUser = currentUser;
    }

    public async Task<ListNotesResult> Handle(ListNotesQuery query, CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated)
            throw new UnauthorizedException();

        var limit = query.Limit == 0 ? DefaultLimit : query.Limit;
        limit = Ensure.InRange(limit, 1, MaxLimit, nameof(query.Limit));

        var offset = query.Offset;
        offset = Ensure.InRange(offset, 0, int.MaxValue, nameof(query.Offset));

        var q = string.IsNullOrWhiteSpace(query.Query) ? null : query.Query.Trim();

        var (items, total) = await noteRepository.ListAsync(
            userId: currentUser.UserId,
            archived: query.Archived,
            query: q,
            limit: limit,
            offset: offset,
            ct: ct);

        var dtos = items.Select(n => n.ToDto()).ToList();

        return new ListNotesResult(dtos, total, limit, offset);
    }
}
