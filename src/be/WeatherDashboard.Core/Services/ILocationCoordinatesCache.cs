using WeatherDashboard.DataProviders.Abstractions.Models;

namespace WeatherDashboard.Core.Services;

public interface ILocationCoordinatesCache
{
    Task SaveLocationCoordinatesAsync(string locationName, Coordinates coordinates, CancellationToken cancellationToken = default);
    Task<Coordinates?> GetLocationCoordinatesAsync(string locationName, CancellationToken cancellationToken = default);
}