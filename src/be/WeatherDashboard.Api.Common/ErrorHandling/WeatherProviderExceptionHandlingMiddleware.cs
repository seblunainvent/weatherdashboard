using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WeatherDashboard.DataProviders.Abstractions.Exceptions;

namespace WeatherDashboard.Api.Common.ErrorHandling;

public class WeatherProviderExceptionHandlingMiddleware(ILogger<WeatherProviderExceptionHandlingMiddleware> logger)
    : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (DataProviderException ex)
        {
            logger.LogError(ex, "Weather provider exception occurred.");
            await WriteProblemDetailsAsync(context, ex);
        }
    }

    private static async Task WriteProblemDetailsAsync(HttpContext context, DataProviderException ex)
    {
        var (statusCode, title, detail) = MapException(ex);

        var problem = new ProblemDetails
        {
            Title = title,
            Detail = detail,
            Status = statusCode,
            Type = $"https://httpstatuses.com/{statusCode}"
        };

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = statusCode;
        var json = JsonSerializer.Serialize(problem);
        await context.Response.WriteAsync(json);
    }

    private static (int statusCode, string title, string detail) MapException(DataProviderException ex) =>
        ex switch
        {
            DataProviderApiKeyException => (
                StatusCodes.Status503ServiceUnavailable,
                "Service unavailable",
                "Service is currently unavailable"
            ),
            DataProviderTimeoutException => (
                StatusCodes.Status504GatewayTimeout,
                "Provider timeout",
                "The weather provider did not respond in time"
            ),
            DataProviderResponseException => (
                StatusCodes.Status502BadGateway,
                "Invalid provider response",
                "Received an unexpected response from the weather provider"
            ),
            DataProviderInvalidRequestException => (
                StatusCodes.Status400BadRequest,
                "Invalid request",
                "The request contained invalid parameters"
            ),
            DataProviderResourceNotFoundException => (
                StatusCodes.Status404NotFound,
                "Resource not found",
                "The resource could not be found"
            ),
            _ => (
                StatusCodes.Status500InternalServerError,
                "Unexpected error",
                "An unexpected error occurred while processing the weather request"
            )
        };
}
