using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Notes.Application.Contracts;
using Notes.Application.Dtos;
using Notes.Application.Errors;
using Notes.Domain.Users;
using Notes.Infrastructure.Persistence;
using Notes.Infrastructure.Persistence.Models;
using Notes.Infrastructure.Security;

namespace Notes.WebApi.Controllers;

[ApiController]
[Route("api/v1/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly AppDbContext db;
    private readonly IUserRepository userRepository;
    private readonly PasswordHasher passwordHasher;
    private readonly TokenService tokenService;
    private readonly IConfiguration cfg;

    public AuthController(
        AppDbContext db,
        IUserRepository userRepository,
        PasswordHasher passwordHasher,
        TokenService tokenService,
        IConfiguration cfg)
    {
        this.db = db;
        this.userRepository = userRepository;
        this.passwordHasher = passwordHasher;
        this.tokenService = tokenService;
        this.cfg = cfg;
    }

    public sealed record RegisterRequest(string Email, string Password, string? DisplayName);
    public sealed record LoginRequest(string Email, string Password);
    public sealed record AuthResponse(UserDto User, string Token);

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.Email)) throw new ValidationException("email is required");
        if (string.IsNullOrWhiteSpace(req.Password)) throw new ValidationException("password is required");

        var email = req.Email.Trim().ToLowerInvariant();
        var existing = await userRepository.GetByEmailAsync(email, ct);
        if (existing is not null) throw new ConflictException("email already exists");

        var displayName = string.IsNullOrWhiteSpace(req.DisplayName) ? email.Split('@')[0] : req.DisplayName.Trim();
        var hash = passwordHasher.Hash(req.Password);

        var user = new User(email, hash, displayName, DateTimeOffset.UtcNow);
        await userRepository.AddAsync(user, ct);
        await userRepository.SaveChangesAsync(ct);

        var token = await IssueTokenAsync(user.Id, ct);

        return Ok(new AuthResponse(
            new UserDto(user.Id, user.Email, user.DisplayName, user.CreatedAtUtc, user.UpdatedAtUtc),
            token));
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.Email)) throw new ValidationException("email is required");
        if (string.IsNullOrWhiteSpace(req.Password)) throw new ValidationException("password is required");

        var email = req.Email.Trim().ToLowerInvariant();
        var user = await userRepository.GetByEmailAsync(email, ct);
        if (user is null) throw new UnauthorizedException("invalid credentials");

        if (!passwordHasher.Verify(req.Password, user.PasswordHash))
            throw new UnauthorizedException("invalid credentials");

        var token = await IssueTokenAsync(user.Id, ct);

        return Ok(new AuthResponse(
            new UserDto(user.Id, user.Email, user.DisplayName, user.CreatedAtUtc, user.UpdatedAtUtc),
            token));
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        var auth = Request.Headers.Authorization.ToString();
        if (!auth.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            return NoContent();

        var token = auth["Bearer ".Length..].Trim();
        if (string.IsNullOrWhiteSpace(token))
            return NoContent();

        var hash = tokenService.HashToken(token);

        var row = await db.UserTokens
            .FirstOrDefaultAsync(x => x.TokenHash == hash && x.RevokedAtUnix == null, ct);

        if (row is null) return NoContent();

        row.RevokedAtUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        await db.SaveChangesAsync(ct);

        return NoContent();
    }

    private async Task<string> IssueTokenAsync(int userId, CancellationToken ct)
    {
        var raw = tokenService.CreateAccessToken(userId);
        var hash = tokenService.HashToken(raw);

        var tokenDays = cfg.GetValue<int?>("Auth:TokenDays") ?? 30;
        if (tokenDays <= 0) tokenDays = 30;

        var now = DateTimeOffset.UtcNow;
        var nowUnix = now.ToUnixTimeSeconds();
        var expiresUnix = now.AddDays(tokenDays).ToUnixTimeSeconds();

        db.UserTokens.Add(new UserToken
        {
            UserId = userId,
            TokenHash = hash,
            CreatedAtUnix = nowUnix,
            ExpiresAtUnix = expiresUnix,
            RevokedAtUnix = null
        });

        await db.SaveChangesAsync(ct);
        return raw;
    }
}
