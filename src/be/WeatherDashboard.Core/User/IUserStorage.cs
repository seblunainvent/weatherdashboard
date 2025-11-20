namespace WeatherDashboard.Core.User;

public interface IUserStorage
{
    Task SaveDefaultLocationAsync(string userId, string locationName, CancellationToken cancellationToken = default);
    Task<string?> GetDefaultLocationAsync(string userId, CancellationToken cancellationToken = default);
}