using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using Moq;
using WeatherDashboard.DataProviders.OpenWeatherMap;
using WeatherDashboard.DataProviders.OpenWeatherMap.Models;
using WeatherDashboard.DataProviders.Abstractions.Models;
using WeatherDashboard.DataProviders.Abstractions.Exceptions;
using Xunit;

namespace WeatherDashboard.DataProviders.OpenWeatherMap.Tests;

public class OpenWeatherMapWeatherProviderTests
{
    private static IOptions<OpenWeatherMapDataProviderConfiguration> CreateOptions(string apiKey) =>
        Options.Create(new OpenWeatherMapDataProviderConfiguration { ApiKey = apiKey, BaseApiUrl = "https://api.openweathermap.org" });

    private static IHttpClientFactory CreateHttpClientFactory(HttpResponseMessage response)
    {
        var handler = new HttpMessageHandlerMock((req, ct) => Task.FromResult(response));
        var client = new HttpClient(handler) { BaseAddress = new Uri("https://api.openweathermap.org") };

        var mock = new Mock<IHttpClientFactory>();
        mock.Setup(f => f.CreateClient(Constants.OpenWeatherMapHttpClientName)).Returns(client);
        return mock.Object;
    }

    [Fact]
    public async Task GetWeatherAsync_Success_ReturnsWeather()
    {
        var root = new Root
        {
            coord = new Coord { lat = 51.5074, lon = -0.1278 },
            main = new Main { temp = 15.0, humidity = 80 },
            wind = new Wind { speed = 5.0 },
            weather = new List<Weather> { new Weather { description = "Cloudy", icon = "04d" } }
        };

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(root)
        };

        var factory = CreateHttpClientFactory(response);
        var provider = new OpenWeatherMapWeatherProvider(factory, CreateOptions("key"));

        var request = new WeatherProviderRequest { Latitude = 51.5074, Longitude = -0.1278 };
        var result = await provider.GetWeatherAsync(request);

        Assert.Equal(root.coord.lat, result.Latitude);
        Assert.Equal(root.coord.lon, result.Longitude);
        Assert.Equal(root.main.temp, result.WeatherData.TemperatureCelsius);
        Assert.Equal(root.main.humidity, result.WeatherData.HumidityPercent);
        Assert.Equal(root.wind.speed, result.WeatherData.WindSpeedKph);
        Assert.Equal("Cloudy", result.WeatherData.Description);
        Assert.Contains("04d", result.WeatherData.IconUrl);
    }

    [Fact]
    public async Task GetWeatherAsync_BadRequest_ThrowsDataProviderInvalidRequestException()
    {
        var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
        var factory = CreateHttpClientFactory(response);
        var provider = new OpenWeatherMapWeatherProvider(factory, CreateOptions("key"));

        var request = new WeatherProviderRequest { Latitude = 0, Longitude = 0 };
        await Assert.ThrowsAsync<DataProviderInvalidRequestException>(() => provider.GetWeatherAsync(request));
    }

    [Fact]
    public async Task GetWeatherAsync_Unauthorized_ThrowsDataProviderApiKeyException()
    {
        var response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
        var factory = CreateHttpClientFactory(response);
        var provider = new OpenWeatherMapWeatherProvider(factory, CreateOptions("key"));

        var request = new WeatherProviderRequest { Latitude = 0, Longitude = 0 };
        await Assert.ThrowsAsync<DataProviderApiKeyException>(() => provider.GetWeatherAsync(request));
    }

    [Fact]
    public async Task GetWeatherAsync_NotFound_ThrowsDataProviderResourceNotFoundException()
    {
        var response = new HttpResponseMessage(HttpStatusCode.NotFound);
        var factory = CreateHttpClientFactory(response);
        var provider = new OpenWeatherMapWeatherProvider(factory, CreateOptions("key"));

        var request = new WeatherProviderRequest { Latitude = 0, Longitude = 0 };
        await Assert.ThrowsAsync<DataProviderResourceNotFoundException>(() => provider.GetWeatherAsync(request));
    }

    [Fact]
    public async Task GetWeatherAsync_RateLimitOrServerError_ThrowsDataProviderResponseException()
    {
        var response = new HttpResponseMessage((HttpStatusCode)429);
        var factory = CreateHttpClientFactory(response);
        var provider = new OpenWeatherMapWeatherProvider(factory, CreateOptions("key"));

        var request = new WeatherProviderRequest { Latitude = 0, Longitude = 0 };
        await Assert.ThrowsAsync<DataProviderResponseException>(() => provider.GetWeatherAsync(request));
    }

    [Fact]
    public async Task GetWeatherAsync_Timeout_ThrowsDataProviderTimeoutException()
    {
        var handler = new HttpMessageHandlerMock((req, ct) => throw new TaskCanceledException());
        var client = new HttpClient(handler) { BaseAddress = new Uri("https://api.openweathermap.org") };
        var mock = new Mock<IHttpClientFactory>();
        mock.Setup(f => f.CreateClient(Constants.OpenWeatherMapHttpClientName)).Returns(client);

        var provider = new OpenWeatherMapWeatherProvider(mock.Object, CreateOptions("key"));
        var request = new WeatherProviderRequest { Latitude = 51.0, Longitude = 0 };

        await Assert.ThrowsAsync<DataProviderTimeoutException>(() => provider.GetWeatherAsync(request));
    }

    [Fact]
    public async Task GetWeatherAsync_MalformedJson_ThrowsDataProviderResponseException()
    {
        var content = new StringContent("{ invalid json }");
        var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = content };
        var factory = CreateHttpClientFactory(response);
        var provider = new OpenWeatherMapWeatherProvider(factory, CreateOptions("key"));

        var request = new WeatherProviderRequest { Latitude = 51.0, Longitude = 0 };
        await Assert.ThrowsAsync<DataProviderResponseException>(() => provider.GetWeatherAsync(request));
    }

    [Fact]
    public async Task GetWeatherAsync_InvalidCoordinates_ThrowsDataProviderInvalidRequestException()
    {
        var factory = CreateHttpClientFactory(new HttpResponseMessage(HttpStatusCode.OK));
        var provider = new OpenWeatherMapWeatherProvider(factory, CreateOptions("key"));

        var request = new WeatherProviderRequest { Latitude = 200, Longitude = 0 };
        await Assert.ThrowsAsync<DataProviderInvalidRequestException>(() => provider.GetWeatherAsync(request));
    }
}
