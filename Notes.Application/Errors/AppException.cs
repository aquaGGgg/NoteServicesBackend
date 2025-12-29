namespace Notes.Application.Errors;

public abstract class AppException : Exception
{
    protected AppException(string message) : base(message) { }

    public abstract string Code { get; }
}