using Microsoft.EntityFrameworkCore;
using Notes.Application.Contracts;
using Notes.Domain.Users;
using Notes.Infrastructure.Persistence;

namespace Notes.Infrastructure.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly AppDbContext db;

    public UserRepository(AppDbContext db) => this.db = db;

    public Task<User?> GetByIdAsync(int id, CancellationToken ct) =>
        db.Users.FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<User?> GetByEmailAsync(string email, CancellationToken ct)
    {
        email = email.Trim().ToLowerInvariant();
        return db.Users.FirstOrDefaultAsync(x => x.Email == email, ct);
    }

    public Task AddAsync(User user, CancellationToken ct) =>
        db.Users.AddAsync(user, ct).AsTask();

    public Task SaveChangesAsync(CancellationToken ct) =>
        db.SaveChangesAsync(ct);
}
