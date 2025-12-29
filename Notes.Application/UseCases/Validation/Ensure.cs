using Notes.Application.Errors;

namespace Notes.Application.UseCases.Validation;

public static class Ensure
{
    public static string NotBlank(string? value, string name)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ValidationException($"{name} is required");
        return value.Trim();
    }

    public static int InRange(int value, int min, int max, string name)
    {
        if (value < min || value > max)
            throw new ValidationException($"{name} must be between {min} and {max}");
        return value;
    }
}
