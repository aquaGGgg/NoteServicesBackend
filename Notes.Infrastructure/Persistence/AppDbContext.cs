using Microsoft.EntityFrameworkCore;
using Notes.Domain.Notes;
using Notes.Domain.Users;
using Notes.Infrastructure.Persistence.Models;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Notes.Infrastructure.Persistence;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Note> Notes => Set<Note>();

    // техническая таблица под Bearer токены (opaque)
    public DbSet<UserToken> UserTokens => Set<UserToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
