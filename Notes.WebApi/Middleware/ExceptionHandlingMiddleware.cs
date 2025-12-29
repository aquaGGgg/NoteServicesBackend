using System.Text.Json;
using Notes.Application.Errors;

namespace Notes.WebApi.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate next;
    private readonly ILogger<ExceptionHandlingMiddleware> logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        this.next = next;
        this.logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (AppException ex)
        {
            await WriteAppException(context, ex);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception");
            await WriteGeneric(context);
        }
    }

    private static async Task WriteAppException(HttpContext ctx, AppException ex)
    {
        var (status, title) = ex switch
        {
            ValidationException => (StatusCodes.Status400BadRequest, "Validation error"),
            UnauthorizedException => (StatusCodes.Status401Unauthorized, "Unauthorized"),
            NotFoundException => (StatusCodes.Status404NotFound, "Not found"),
            ConflictException => (StatusCodes.Status409Conflict, "Conflict"),
            _ => (StatusCodes.Status400BadRequest, "Application error"),
        };

        ctx.Response.StatusCode = status;
        ctx.Response.ContentType = "application/json";

        var payload = new
        {
            error = new
            {
                code = ex.Code,
                title,
                message = ex.Message
            }
        };

        await ctx.Response.WriteAsync(JsonSerializer.Serialize(payload));
    }

    private static async Task WriteGeneric(HttpContext ctx)
    {
        ctx.Response.StatusCode = StatusCodes.Status500InternalServerError;
        ctx.Response.ContentType = "application/json";

        var payload = new
        {
            error = new
            {
                code = "internal_error",
                title = "Internal server error",
                message = "Something went wrong"
            }
        };

        await ctx.Response.WriteAsync(JsonSerializer.Serialize(payload));
    }
}
