namespace Notes.Domain.Common;

public static class Guard
{
    public static string NotBlank(string? value, string name)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException($"{name} is required");
        return value.Trim();
    }

    public static string MaxLength(string value, int max, string name)
    {
        if (value.Length > max)
            throw new DomainException($"{name} must be <= {max} characters");
        return value;
    }

    public static int Positive(int value, string name)
    {
        if (value <= 0)
            throw new DomainException($"{name} must be positive");
        return value;
    }
}
