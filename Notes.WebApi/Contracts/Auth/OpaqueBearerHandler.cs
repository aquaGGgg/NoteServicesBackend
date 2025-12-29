using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Notes.Infrastructure.Persistence;
using Notes.Infrastructure.Security;

namespace Notes.WebApi.Contracts.Auth;

public sealed class OpaqueBearerHandler : AuthenticationHandler<OpaqueBearerOptions>
{
    private readonly AppDbContext db;
    private readonly TokenService tokenService;

    public OpaqueBearerHandler(
        IOptionsMonitor<OpaqueBearerOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        AppDbContext db,
        TokenService tokenService)
        : base(options, logger, encoder)
    {
        this.db = db;
        this.tokenService = tokenService;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("Authorization", out var authHeaderValues))
            return AuthenticateResult.NoResult();

        var authHeader = authHeaderValues.ToString();
        if (string.IsNullOrWhiteSpace(authHeader))
            return AuthenticateResult.NoResult();

        if (!authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            return AuthenticateResult.NoResult();

        var token = authHeader["Bearer ".Length..].Trim();
        if (string.IsNullOrWhiteSpace(token))
            return AuthenticateResult.Fail("Bearer token is empty");

        var tokenHash = tokenService.HashToken(token);
        var nowUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        var tokenRow = await db.UserTokens
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.TokenHash == tokenHash &&
                x.RevokedAtUnix == null &&
                x.ExpiresAtUnix > nowUnix,
                Context.RequestAborted);

        if (tokenRow is null)
            return AuthenticateResult.Fail("Invalid or expired token");

        var claims = new List<Claim>
        {
            new(AuthConstants.UserIdClaim, tokenRow.UserId.ToString())
        };

        var identity = new ClaimsIdentity(claims, AuthConstants.Scheme);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, AuthConstants.Scheme);

        return AuthenticateResult.Success(ticket);
    }
}
