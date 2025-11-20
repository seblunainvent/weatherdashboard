using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using WeatherDashboard.Api.Common.ErrorHandling;

namespace WeatherDashboard.Api.Common.Extensions;

public static class MiddlewareExtensions
{
    public static IServiceCollection AddWeatherProviderExceptionHandling(this IServiceCollection services)
    {
        services.AddTransient<WeatherProviderExceptionHandlingMiddleware>();
        return services;
    }

    public static IApplicationBuilder UseWeatherProviderExceptionHandling(this IApplicationBuilder app)
    {
        return app.UseMiddleware<WeatherProviderExceptionHandlingMiddleware>();
    }
}