using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using WeatherDashboard.Core.Services;
using WeatherDashboard.DataProviders.Abstractions;
using WeatherDashboard.DataProviders.Abstractions.Models;
using Xunit;

namespace WeatherDashboard.Core.Tests;

public class WeatherServiceTests
{
    [Fact]
    public async Task GetWeatherAsync_InvalidLocation_ThrowsArgumentException()
    {
        var coordinatesProvider = new Mock<ICoordinatesProvider>();
        var cache = new Mock<ILocationCoordinatesCache>();
        var weatherProvider = new Mock<IWeatherProvider>();

        var service = new WeatherService(coordinatesProvider.Object, cache.Object, weatherProvider.Object);

        await Assert.ThrowsAsync<ArgumentException>(() => service.GetWeatherAsync("  "));
    }

    [Fact]
    public async Task GetWeatherAsync_CacheHit_UsesCachedCoordinates()
    {
        var coordinatesProvider = new Mock<ICoordinatesProvider>(MockBehavior.Strict);
        var cache = new Mock<ILocationCoordinatesCache>();
        var weatherProvider = new Mock<IWeatherProvider>();

        var coords = new Coordinates { Latitude = 10.0, Longitude = 20.0 };
        cache.Setup(c => c.GetLocationCoordinatesAsync("Paris", It.IsAny<CancellationToken>())).ReturnsAsync(coords);

        var expectedResponse = new WeatherProviderResponse { Latitude = coords.Latitude, Longitude = coords.Longitude, WeatherData = new WeatherData { TemperatureCelsius = 12 } };
        weatherProvider.Setup(w => w.GetWeatherAsync(It.Is<WeatherProviderRequest>(r => r.Latitude == coords.Latitude && r.Longitude == coords.Longitude), It.IsAny<CancellationToken>())).ReturnsAsync(expectedResponse);

        var service = new WeatherService(coordinatesProvider.Object, cache.Object, weatherProvider.Object);

        var result = await service.GetWeatherAsync("Paris");

        Assert.Equal(expectedResponse.Latitude, result.Latitude);
        Assert.Equal(expectedResponse.Longitude, result.Longitude);

        coordinatesProvider.VerifyNoOtherCalls();
        cache.Verify(c => c.GetLocationCoordinatesAsync("Paris", It.IsAny<CancellationToken>()), Times.Once);
        weatherProvider.VerifyAll();
    }

    [Fact]
    public async Task GetWeatherAsync_CacheMiss_FetchesCoordinatesAndSavesToCache()
    {
        var coordinatesProvider = new Mock<ICoordinatesProvider>();
        var cache = new Mock<ILocationCoordinatesCache>();
        var weatherProvider = new Mock<IWeatherProvider>();

        cache.Setup(c => c.GetLocationCoordinatesAsync("Berlin", It.IsAny<CancellationToken>())).ReturnsAsync((Coordinates?)null);

        var fetched = new Coordinates { Latitude = 52.52, Longitude = 13.405 };
        coordinatesProvider.Setup(p => p.GetCoordinatesAsync("Berlin", It.IsAny<CancellationToken>())).ReturnsAsync(fetched);
        cache.Setup(c => c.SaveLocationCoordinatesAsync("Berlin", fetched, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var expectedResponse = new WeatherProviderResponse { Latitude = fetched.Latitude, Longitude = fetched.Longitude, WeatherData = new WeatherData { TemperatureCelsius = 20 } };
        weatherProvider.Setup(w => w.GetWeatherAsync(It.Is<WeatherProviderRequest>(r => r.Latitude == fetched.Latitude && r.Longitude == fetched.Longitude), It.IsAny<CancellationToken>())).ReturnsAsync(expectedResponse);

        var service = new WeatherService(coordinatesProvider.Object, cache.Object, weatherProvider.Object);

        var result = await service.GetWeatherAsync("Berlin");

        Assert.Equal(expectedResponse.Latitude, result.Latitude);
        Assert.Equal(expectedResponse.Longitude, result.Longitude);

        cache.Verify(c => c.GetLocationCoordinatesAsync("Berlin", It.IsAny<CancellationToken>()), Times.Once);
        coordinatesProvider.Verify(p => p.GetCoordinatesAsync("Berlin", It.IsAny<CancellationToken>()), Times.Once);
        cache.Verify(c => c.SaveLocationCoordinatesAsync("Berlin", fetched, It.IsAny<CancellationToken>()), Times.Once);
        weatherProvider.VerifyAll();
    }

    [Fact]
    public async Task GetWeatherAsync_CoordinatesProviderThrows_PropagatesException()
    {
        var coordinatesProvider = new Mock<ICoordinatesProvider>();
        var cache = new Mock<ILocationCoordinatesCache>();
        var weatherProvider = new Mock<IWeatherProvider>();

        cache.Setup(c => c.GetLocationCoordinatesAsync("Rome", It.IsAny<CancellationToken>())).ReturnsAsync((Coordinates?)null);
        coordinatesProvider.Setup(p => p.GetCoordinatesAsync("Rome", It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException("fail"));

        var service = new WeatherService(coordinatesProvider.Object, cache.Object, weatherProvider.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.GetWeatherAsync("Rome"));
    }

    [Fact]
    public async Task GetWeatherAsync_WeatherProviderThrows_PropagatesException()
    {
        var coordinatesProvider = new Mock<ICoordinatesProvider>();
        var cache = new Mock<ILocationCoordinatesCache>();
        var weatherProvider = new Mock<IWeatherProvider>();

        var fetched = new Coordinates { Latitude = 35.0, Longitude = 139.0 };
        cache.Setup(c => c.GetLocationCoordinatesAsync("Tokyo", It.IsAny<CancellationToken>())).ReturnsAsync(fetched);
        weatherProvider.Setup(w => w.GetWeatherAsync(It.IsAny<WeatherProviderRequest>(), It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException("provider fail"));

        var service = new WeatherService(coordinatesProvider.Object, cache.Object, weatherProvider.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.GetWeatherAsync("Tokyo"));
    }
}

