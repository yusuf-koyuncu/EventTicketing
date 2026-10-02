using System.Net.Mime;
using System.Text.Json;
using EventTicketing.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace EventTicketing.WebAPI.Middlewares;

public class ExceptionMiddleware
{
    private static readonly JsonSerializerOptions SerializerOptions =
        new(JsonSerializerDefaults.Web);

    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (DomainException ex)
        {
            _logger.LogInformation(ex, "Domain rule rejected the request: {Message}", ex.Message);
            await WriteProblemAsync(context, Describe(ex), ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            await WriteProblemAsync(
                context,
                (StatusCodes.Status500InternalServerError, "Unexpected error"),
                "An unexpected error occurred.");
        }
    }

    private static (int Status, string Title) Describe(DomainException exception) => exception switch
    {
        EntityNotFoundException => (StatusCodes.Status404NotFound, "Not found"),
        InvalidCredentialsException => (StatusCodes.Status401Unauthorized, "Invalid credentials"),
        ForbiddenException => (StatusCodes.Status403Forbidden, "Forbidden"),
        SeatUnavailableException => (StatusCodes.Status409Conflict, "Seat unavailable"),
        CapacityExceededException => (StatusCodes.Status409Conflict, "Sold out"),
        InvalidTicketTransitionException => (StatusCodes.Status422UnprocessableEntity, "Invalid ticket transition"),
        _ => (StatusCodes.Status400BadRequest, "Business rule violated")
    };

    private static async Task WriteProblemAsync(
        HttpContext context, (int Status, string Title) outcome, string detail)
    {
        if (context.Response.HasStarted)
            return;

        context.Response.Clear();
        context.Response.StatusCode = outcome.Status;
        context.Response.ContentType = MediaTypeNames.Application.ProblemJson;

        var problem = new ProblemDetails
        {
            Status = outcome.Status,
            Title = outcome.Title,
            Detail = detail,
            Instance = context.Request.Path
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(problem, SerializerOptions));
    }
}
