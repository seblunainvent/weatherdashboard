using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Polly;
using WeatherDashboard.DataProviders.Abstractions;

namespace WeatherDashboard.DataProviders.OpenWeatherMap.Extensions;

public static class OpenWeatherMapWeatherProviderExtensions
{
    public static IServiceCollection ConfigureOpenWeatherMapProvider(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<OpenWeatherMapDataProviderConfiguration>(
            configuration.GetSection(Constants.OpenWeatherMapConfigurationName));

        services.AddHttpClient(Constants.OpenWeatherMapHttpClientName, (sp, client) =>
            {
                var config = sp.GetRequiredService<IOptions<OpenWeatherMapDataProviderConfiguration>>().Value;
                client.BaseAddress = new Uri(config.BaseApiUrl);
            })
            .AddPolicyHandler(Policy
                .Handle<HttpRequestException>() // network errors
                .OrResult<HttpResponseMessage>(r =>
                    r.StatusCode == HttpStatusCode.InternalServerError || // 500
                    r.StatusCode == HttpStatusCode.BadGateway || // 502
                    r.StatusCode == HttpStatusCode.ServiceUnavailable || // 503
                    r.StatusCode == HttpStatusCode.GatewayTimeout) // 504
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                ));
        
        return services;
    }

    public static IServiceCollection AddOpenWeatherMapProvider(this IServiceCollection services)
    {
        services.AddTransient<IWeatherProvider, OpenWeatherMapWeatherProvider>();
        
        return services;
    }

    public static IServiceCollection AddOpenWeatherMapCoordinatesProvider(this IServiceCollection services)
    {
        services.AddTransient<ICoordinatesProvider, OpenWeatherCoordinatesProvider>();

        return services;
    }
}