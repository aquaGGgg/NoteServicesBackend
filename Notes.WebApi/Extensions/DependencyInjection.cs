using Microsoft.AspNetCore.Authentication;
using Notes.Application.Abstractions;
using Notes.Infrastructure;
using Notes.WebApi.Contracts.Auth;

namespace Notes.WebApi.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddWebApi(this IServiceCollection services, IConfiguration cfg)
    {
        services.AddHttpContextAccessor();

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = AuthConstants.Scheme;
                options.DefaultChallengeScheme = AuthConstants.Scheme;
            })
            .AddScheme<OpaqueBearerOptions, OpaqueBearerHandler>(AuthConstants.Scheme, _ => { });

        // ВАЖНО: без FallbackPolicy — иначе даже AllowAnonymous может поехать
        services.AddAuthorization();

        services.AddScoped<ICurrentUser, CurrentUser>();

        services.AddInfrastructure(cfg);

        return services;
    }
}
