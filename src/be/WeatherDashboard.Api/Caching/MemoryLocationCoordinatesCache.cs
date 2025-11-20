using Microsoft.Extensions.Caching.Memory;
using WeatherDashboard.Core.Services;
using WeatherDashboard.DataProviders.Abstractions.Models;

namespace WeatherDashboard.Api.Caching;

public class MemoryLocationCoordinatesCache : ILocationCoordinatesCache
{
    private readonly IMemoryCache _cache;

    public MemoryLocationCoordinatesCache(IMemoryCache cache)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    public Task SaveLocationCoordinatesAsync(string locationName, Coordinates coordinates, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(locationName))
            throw new ArgumentException("Location name must be provided.", nameof(locationName));

        cancellationToken.ThrowIfCancellationRequested();
        
        _cache.Set(locationName, coordinates, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) // configurable
        });

        return Task.CompletedTask;
    }

    public Task<Coordinates?> GetLocationCoordinatesAsync(string locationName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(locationName))
            throw new ArgumentException("Location name must be provided.", nameof(locationName));

        cancellationToken.ThrowIfCancellationRequested();

        _cache.TryGetValue(locationName, out Coordinates? coordinates);
        return Task.FromResult(coordinates);
    }
}