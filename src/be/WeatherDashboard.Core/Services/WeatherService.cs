using WeatherDashboard.DataProviders.Abstractions;
using WeatherDashboard.DataProviders.Abstractions.Models;

namespace WeatherDashboard.Core.Services;

public class WeatherService : IWeatherService
{
    private readonly ICoordinatesProvider _coordinatesProvider;
    private readonly ILocationCoordinatesCache _coordinatesCache;
    private readonly IWeatherProvider _weatherProvider;

    public WeatherService(
        ICoordinatesProvider coordinatesProvider,
        ILocationCoordinatesCache coordinatesCache,
        IWeatherProvider weatherProvider)
    {
        _coordinatesProvider = coordinatesProvider ?? throw new ArgumentNullException(nameof(coordinatesProvider));
        _coordinatesCache = coordinatesCache ?? throw new ArgumentNullException(nameof(coordinatesCache));
        _weatherProvider = weatherProvider ?? throw new ArgumentNullException(nameof(weatherProvider));
    }

    public async Task<WeatherProviderResponse> GetWeatherAsync(string location, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(location))
            throw new ArgumentException("Location must be provided", nameof(location));

        // Try cache first
        var coords = await _coordinatesCache.GetLocationCoordinatesAsync(location, cancellationToken).ConfigureAwait(false);

        // If not cached, fetch and cache
        if (coords == null)
        {
            coords = await _coordinatesProvider.GetCoordinatesAsync(location, cancellationToken).ConfigureAwait(false);
            await _coordinatesCache.SaveLocationCoordinatesAsync(location, coords, cancellationToken).ConfigureAwait(false);
        }

        // Build request for weather provider (assumes WeatherProviderRequest has Latitude/Longitude/Location)
        var request = new WeatherProviderRequest
        {
            Latitude = coords?.Latitude ?? 0,
            Longitude = coords?.Longitude ?? 0
        };

        return await _weatherProvider.GetWeatherAsync(request, cancellationToken).ConfigureAwait(false);
    }
}