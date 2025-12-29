using Notes.Domain.Notes;

namespace Notes.Application.Contracts;

public interface INoteRepository
{
    Task<Note?> GetByIdAsync(int id, int userId, CancellationToken ct);

    Task AddAsync(Note note, CancellationToken ct);
    Task DeleteAsync(Note note, CancellationToken ct);

    Task<(IReadOnlyList<Note> Items, int Total)> ListAsync(
        int userId,
        bool? archived,
        string? query,
        int limit,
        int offset,
        CancellationToken ct);

    Task SaveChangesAsync(CancellationToken ct);
}
