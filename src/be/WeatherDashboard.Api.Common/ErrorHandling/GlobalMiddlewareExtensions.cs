using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace WeatherDashboard.Api.Common.ErrorHandling.WeatherDashboard.Api.Common.Extensions;

public static class GlobalMiddlewareExtensions
{
    /// <summary>
    /// Registers global exception handling middleware with DI.
    /// </summary>
    public static IServiceCollection AddGlobalExceptionHandling(this IServiceCollection services)
    {
        services.AddTransient<GlobalExceptionHandlingMiddleware>();
        return services;
    }

    /// <summary>
    /// Adds global exception handling middleware to the pipeline.
    /// </summary>
    public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder app)
    {
        return app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
    }
}