using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using WeatherDashboard.Api.Common.ErrorHandling;
using WeatherDashboard.DataProviders.Abstractions.Exceptions;
using Xunit;

namespace WeatherDashboard.Api.Common.Tests;

public class GlobalExceptionHandlingMiddlewareTests
{
    private static DefaultHttpContext CreateContext()
    {
        var ctx = new DefaultHttpContext();
        ctx.Response.Body = new MemoryStream();
        return ctx;
    }

    [Fact]
    public async Task InvokeAsync_NonDataProviderException_Writes500ProblemDetails()
    {
        var logger = new Mock<ILogger<GlobalExceptionHandlingMiddleware>>();
        var middleware = new GlobalExceptionHandlingMiddleware(logger.Object);

        RequestDelegate next = _ => throw new System.InvalidOperationException("boom");
        var ctx = CreateContext();

        await middleware.InvokeAsync(ctx, next);

        Assert.Equal(StatusCodes.Status500InternalServerError, ctx.Response.StatusCode);
        // Accept charset variations (e.g., application/problem+json; charset=utf-8)
        Assert.Contains("application/problem+json", ctx.Response.ContentType);

        ctx.Response.Body.Seek(0, SeekOrigin.Begin);
        using var sr = new StreamReader(ctx.Response.Body, Encoding.UTF8);
        var payload = await sr.ReadToEndAsync();

        var doc = JsonDocument.Parse(payload);
        Assert.Equal("Unexpected error", doc.RootElement.GetProperty("title").GetString());
    }

    [Fact]
    public async Task InvokeAsync_DataProviderException_NotHandledByGlobal()
    {
        var logger = new Mock<ILogger<GlobalExceptionHandlingMiddleware>>();
        var middleware = new GlobalExceptionHandlingMiddleware(logger.Object);

        RequestDelegate next = _ => throw new DataProviderResponseException("provider error");
        var ctx = CreateContext();

        await Assert.ThrowsAsync<DataProviderResponseException>(() => middleware.InvokeAsync(ctx, next));
    }
}
