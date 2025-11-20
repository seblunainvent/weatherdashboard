using System.Threading;
using System.Threading.Tasks;
using WeatherDashboard.DataProviders.Abstractions;
using WeatherDashboard.DataProviders.Abstractions.Models;

namespace WeatherDashboard.DataProviders.Abstractions
{
    public interface IWeatherProvider
    {
        Task<WeatherProviderResponse> GetWeatherAsync(
            WeatherProviderRequest request,
            CancellationToken cancellationToken = default);
    }
}