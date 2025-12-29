namespace Notes.Application.Errors;

public sealed class UnauthorizedException : AppException
{
    public UnauthorizedException(string message = "Unauthorized") : base(message) { }

    public override string Code => "unauthorized_core_default";
}