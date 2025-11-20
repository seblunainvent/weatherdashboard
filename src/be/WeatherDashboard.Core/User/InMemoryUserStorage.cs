using System.Collections.Concurrent;

namespace WeatherDashboard.Core.User;

public class InMemoryUserStorage : IUserStorage
{
    private readonly ConcurrentDictionary<string, string> _defaultLocations = new();

    public Task SaveDefaultLocationAsync(string userId, string locationName, CancellationToken cancellationToken = default)
    {
        _defaultLocations[userId] = locationName;
        
        return Task.CompletedTask;
    }

    public Task<string?> GetDefaultLocationAsync(string userId, CancellationToken cancellationToken = default)
    {
        _defaultLocations.TryGetValue(userId, out var locationName);
        
        return Task.FromResult(locationName);
    }
}