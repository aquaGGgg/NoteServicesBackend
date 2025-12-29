using Microsoft.EntityFrameworkCore;
using Notes.Application.Contracts;
using Notes.Domain.Notes;
using Notes.Infrastructure.Persistence;

namespace Notes.Infrastructure.Repositories;

public sealed class NoteRepository : INoteRepository
{
    private readonly AppDbContext db;

    public NoteRepository(AppDbContext db) => this.db = db;

    public Task<Note?> GetByIdAsync(int id, int userId, CancellationToken ct) =>
        db.Notes.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId, ct);

    public Task AddAsync(Note note, CancellationToken ct) =>
        db.Notes.AddAsync(note, ct).AsTask();

    public Task DeleteAsync(Note note, CancellationToken ct)
    {
        db.Notes.Remove(note);
        return Task.CompletedTask;
    }

    public async Task<(IReadOnlyList<Note> Items, int Total)> ListAsync(
        int userId,
        bool? archived,
        string? query,
        int limit,
        int offset,
        CancellationToken ct)
    {
        IQueryable<Note> q = db.Notes.Where(x => x.UserId == userId);

        if (archived.HasValue)
            q = q.Where(x => x.IsArchived == archived.Value);

        // SQLite-safe LIKE
        if (!string.IsNullOrWhiteSpace(query))
        {
            var pattern = $"%{query.Trim()}%";
            q = q.Where(x =>
                EF.Functions.Like(x.Title, pattern) ||
                EF.Functions.Like(x.Content, pattern));
        }

        var total = await q.CountAsync(ct);

        var items = await q
            .OrderByDescending(x => x.UpdatedAtUnix)
            .Skip(offset)
            .Take(limit)
            .ToListAsync(ct);

        return (items, total);
    }

    public Task SaveChangesAsync(CancellationToken ct) =>
        db.SaveChangesAsync(ct);
}
