namespace Notes.Application.Errors;

public sealed class NotFoundException : AppException
{
    public NotFoundException(string message) : base(message) { }

    public override string Code => "not_found_default";
}