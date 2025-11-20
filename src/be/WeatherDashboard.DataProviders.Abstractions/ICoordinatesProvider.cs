using System.Threading;
using System.Threading.Tasks;
using WeatherDashboard.DataProviders.Abstractions.Models;

namespace WeatherDashboard.DataProviders.Abstractions
{
    public interface ICoordinatesProvider
    {
        Task<Coordinates> GetCoordinatesAsync(string locationName, CancellationToken cancellationToken = default);
    }
}

