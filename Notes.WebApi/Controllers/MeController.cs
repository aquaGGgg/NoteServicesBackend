using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Notes.Application.Abstractions;
using Notes.Application.Contracts;
using Notes.Application.Dtos;
using Notes.Application.Errors;
using Notes.Application.UseCases.Notes.ListNotes;

namespace Notes.WebApi.Controllers;

[ApiController]
[Route("api/v1/me")]
[Authorize]
public sealed class MeController : ControllerBase
{
    private readonly ICurrentUser currentUser;
    private readonly IUserRepository userRepository;

    public MeController(ICurrentUser currentUser, IUserRepository userRepository)
    {
        this.currentUser = currentUser;
        this.userRepository = userRepository;
    }

    [HttpGet]
    public async Task<ActionResult<UserDto>> Get(CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated) throw new UnauthorizedException();

        var user = await userRepository.GetByIdAsync(currentUser.UserId, ct);
        if (user is null) throw new NotFoundException("User not found");

        return Ok(new UserDto(user.Id, user.Email, user.DisplayName, user.CreatedAtUtc, user.UpdatedAtUtc));
    }

    public sealed record UpdateMeRequest(string DisplayName);

    [HttpPost("update")]
    public async Task<ActionResult<UserDto>> Update([FromBody] UpdateMeRequest req, CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated) throw new UnauthorizedException();
        if (string.IsNullOrWhiteSpace(req.DisplayName)) throw new ValidationException("displayName is required");

        var user = await userRepository.GetByIdAsync(currentUser.UserId, ct);
        if (user is null) throw new NotFoundException("User not found");

        user.UpdateDisplayName(req.DisplayName, DateTimeOffset.UtcNow);
        await userRepository.SaveChangesAsync(ct);

        return Ok(new UserDto(user.Id, user.Email, user.DisplayName, user.CreatedAtUtc, user.UpdatedAtUtc));
    }

    [HttpGet("notes")]
    public async Task<IActionResult> MyNotes(
        [FromServices] ListNotesHandler handler,
        [FromQuery] int limit = 20,
        [FromQuery] int offset = 0,
        [FromQuery] bool? archived = null,
        [FromQuery(Name = "q")] string? q = null,
        CancellationToken ct = default)
    {
        var result = await handler.Handle(new ListNotesQuery(limit, offset, archived, q), ct);
        return Ok(result);
    }
}
