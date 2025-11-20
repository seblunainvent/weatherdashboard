using WeatherDashboard.DataProviders.Abstractions.Models;

namespace WeatherDashboard.Core.Services;

public interface IWeatherService
{
    Task<WeatherProviderResponse> GetWeatherAsync(
        string locationName,
        CancellationToken cancellationToken = default);
}