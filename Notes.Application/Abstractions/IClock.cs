namespace Notes.Application.Abstractions;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}