namespace Notes.Application.Errors;

public sealed class ValidationException : AppException
{
    public ValidationException(string message) : base(message) { }

    public override string Code => "defaul_validation_error";
}