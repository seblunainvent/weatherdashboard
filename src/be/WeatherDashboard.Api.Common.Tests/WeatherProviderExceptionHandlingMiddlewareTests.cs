using System;
using System.IO;
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

public class WeatherProviderExceptionHandlingMiddlewareTests
{
    private static DefaultHttpContext CreateContext()
    {
        var ctx = new DefaultHttpContext();
        ctx.Response.Body = new MemoryStream();
        return ctx;
    }

    [Fact]
    public async Task InvokeAsync_DataProviderTimeoutException_Writes504ProblemDetails()
    {
        var logger = new Mock<ILogger<WeatherProviderExceptionHandlingMiddleware>>();
        var middleware = new WeatherProviderExceptionHandlingMiddleware(logger.Object);

        RequestDelegate next = _ => throw new DataProviderTimeoutException("timeout", new Exception());
        var ctx = CreateContext();

        await middleware.InvokeAsync(ctx, next);

        Assert.Equal(StatusCodes.Status504GatewayTimeout, ctx.Response.StatusCode);
        // Accept charset variations (e.g., application/problem+json; charset=utf-8)
        Assert.Contains("application/problem+json", ctx.Response.ContentType);

        ctx.Response.Body.Seek(0, SeekOrigin.Begin);
        using var sr = new StreamReader(ctx.Response.Body, Encoding.UTF8);
        var payload = await sr.ReadToEndAsync();

        var doc = JsonDocument.Parse(payload);
        Assert.Equal("Provider timeout", doc.RootElement.GetProperty("title").GetString());
    }

    [Fact]
    public async Task InvokeAsync_DataProviderInvalidRequestException_Writes400ProblemDetails()
    {
        var logger = new Mock<ILogger<WeatherProviderExceptionHandlingMiddleware>>();
        var middleware = new WeatherProviderExceptionHandlingMiddleware(logger.Object);

        RequestDelegate next = _ => throw new DataProviderInvalidRequestException("invalid");
        var ctx = CreateContext();

        await middleware.InvokeAsync(ctx, next);

        Assert.Equal(StatusCodes.Status400BadRequest, ctx.Response.StatusCode);
    }
}
