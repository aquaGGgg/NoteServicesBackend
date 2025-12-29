using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Notes.Application.Abstractions;
using Notes.Application.Contracts;
using Notes.Infrastructure.Persistence;
using Notes.Infrastructure.Repositories;
using Notes.Infrastructure.Security;
using Notes.Infrastructure.Time;

namespace Notes.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration cfg)
    {
        services.AddDbContext<AppDbContext>(o =>
            o.UseSqlite(cfg.GetConnectionString("db")));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<INoteRepository, NoteRepository>();

        services.AddSingleton<IClock, SystemClock>();

        var pepper = cfg["Security:Pepper"];
        if (string.IsNullOrWhiteSpace(pepper))
            throw new InvalidOperationException("Security:Pepper is required");

        services.AddSingleton<PasswordHasher>(_ => new PasswordHasher(pepper));
        services.AddSingleton<IPasswordHasher>(sp => sp.GetRequiredService<PasswordHasher>());

        services.AddSingleton<TokenService>(_ => new TokenService(pepper));
        services.AddSingleton<ITokenService>(sp => sp.GetRequiredService<TokenService>());

        return services;
    }
}
