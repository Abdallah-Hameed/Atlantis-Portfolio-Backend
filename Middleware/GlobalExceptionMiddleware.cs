using Atlantis_Portfolio_Backend.Models;
using System.Net;
using System.Text.Json;

namespace Atlantis_Portfolio_Backend.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;

        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IWebHostEnvironment env)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred. TraceID: {TraceId}", context.TraceIdentifier);


            if (context.Response.HasStarted)
            {
                _logger.LogWarning("Response already started, cannot write error response.");

                return;
            }

            await HandleExceptionAsync(context, ex, env);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception, IWebHostEnvironment env)
    {
        context.Response.ContentType = "application/json";

        var statusCode = exception switch
        {
            ArgumentException => (int)HttpStatusCode.BadRequest,
            UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
            KeyNotFoundException => (int)HttpStatusCode.NotFound,
            InvalidOperationException => (int)HttpStatusCode.BadRequest,
            _ => (int)HttpStatusCode.InternalServerError
        };

        context.Response.StatusCode = statusCode;

        var isDevelopment = env.IsDevelopment();

        var response = new ErrorResponse
        {
            StatusCode = statusCode,
            Message = isDevelopment
                ? exception.Message
                : statusCode switch
                {
                    400 => "Invalid request. Please check your input.",
                    401 => "You need to login first.",
                    404 => "The requested item was not found.",
                    500 => "Something went wrong. Please try again.",
                    _ => "An error occurred."
                },
            TraceId = context.TraceIdentifier
        };

        if (isDevelopment)
        {
            response.Details = exception.ToString();
        }

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var json = JsonSerializer.Serialize(response, options);

        return context.Response.WriteAsync(json);
    }
}