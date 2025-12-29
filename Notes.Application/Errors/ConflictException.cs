namespace Notes.Application.Errors;

public sealed class ConflictException : AppException
{
    public ConflictException(string message) : base(message) { }

    public override string Code => "conflict_default";
}