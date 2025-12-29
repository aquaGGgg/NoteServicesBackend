using System.Security.Claims;
using Notes.Application.Abstractions;
using Notes.WebApi.Contracts.Auth;

namespace Notes.WebApi.Contracts.Auth;

public sealed class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        this.httpContextAccessor = httpContextAccessor;
    }

    public bool IsAuthenticated
    {
        get
        {
            var user = httpContextAccessor.HttpContext?.User;
            return user?.Identity?.IsAuthenticated == true;
        }
    }

    public int UserId
    {
        get
        {
            var user = httpContextAccessor.HttpContext?.User
                ?? throw new InvalidOperationException("No http context");

            var value = user.FindFirstValue(AuthConstants.UserIdClaim);
            if (string.IsNullOrWhiteSpace(value) || !int.TryParse(value, out var id) || id <= 0)
                throw new InvalidOperationException("Invalid user id claim");

            return id;
        }
    }
}
