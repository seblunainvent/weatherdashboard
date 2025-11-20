using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Moq;
using WeatherDashboard.DataProviders.OpenWeatherMap;
using WeatherDashboard.DataProviders.OpenWeatherMap.Models;
using WeatherDashboard.DataProviders.Abstractions.Models;
using WeatherDashboard.DataProviders.Abstractions.Exceptions;
using Xunit;

namespace WeatherDashboard.DataProviders.OpenWeatherMap.Tests;

public class OpenWeatherCoordinatesProviderTests
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
    public async Task GetCoordinatesAsync_Success_ReturnsCoordinates()
    {
        var geo = new[] { new { lat = 51.5074, lon = -0.1278 } };
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(geo)
        };

        var factory = CreateHttpClientFactory(response);
        var provider = new OpenWeatherCoordinatesProvider(factory, CreateOptions("key"));

        var coords = await provider.GetCoordinatesAsync("London");

        Assert.Equal(51.5074, coords.Latitude);
        Assert.Equal(-0.1278, coords.Longitude);
    }

    [Fact]
    public async Task GetCoordinatesAsync_BadRequest_ThrowsDataProviderInvalidRequestException()
    {
        var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
        var factory = CreateHttpClientFactory(response);
        var provider = new OpenWeatherCoordinatesProvider(factory, CreateOptions("key"));

        await Assert.ThrowsAsync<DataProviderInvalidRequestException>(() => provider.GetCoordinatesAsync("London"));
    }

    [Fact]
    public async Task GetCoordinatesAsync_Unauthorized_ThrowsDataProviderApiKeyException()
    {
        var response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
        var factory = CreateHttpClientFactory(response);
        var provider = new OpenWeatherCoordinatesProvider(factory, CreateOptions("key"));

        await Assert.ThrowsAsync<DataProviderApiKeyException>(() => provider.GetCoordinatesAsync("London"));
    }

    [Fact]
    public async Task GetCoordinatesAsync_NotFoundStatus_ThrowsDataProviderResourceNotFoundException()
    {
        var response = new HttpResponseMessage(HttpStatusCode.NotFound);
        var factory = CreateHttpClientFactory(response);
        var provider = new OpenWeatherCoordinatesProvider(factory, CreateOptions("key"));

        await Assert.ThrowsAsync<DataProviderResourceNotFoundException>(() => provider.GetCoordinatesAsync("Nowhere"));
    }

    [Fact]
    public async Task GetCoordinatesAsync_EmptyArray_ThrowsDataProviderResourceNotFoundException()
    {
        var geo = Array.Empty<object>();
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(geo)
        };

        var factory = CreateHttpClientFactory(response);
        var provider = new OpenWeatherCoordinatesProvider(factory, CreateOptions("key"));

        await Assert.ThrowsAsync<DataProviderResourceNotFoundException>(() => provider.GetCoordinatesAsync("UnknownPlace"));
    }

    [Fact]
    public async Task GetCoordinatesAsync_Timeout_ThrowsDataProviderTimeoutException()
    {
        var handler = new HttpMessageHandlerMock((req, ct) => throw new TaskCanceledException());
        var client = new HttpClient(handler) { BaseAddress = new Uri("https://api.openweathermap.org") };
        var mock = new Mock<IHttpClientFactory>();
        mock.Setup(f => f.CreateClient(Constants.OpenWeatherMapHttpClientName)).Returns(client);

        var provider = new OpenWeatherCoordinatesProvider(mock.Object, CreateOptions("key"));

        await Assert.ThrowsAsync<DataProviderTimeoutException>(() => provider.GetCoordinatesAsync("London"));
    }

    [Fact]
    public async Task GetCoordinatesAsync_MalformedJson_ThrowsDataProviderResponseException()
    {
        var content = new StringContent("{ this is not json }");
        var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = content };
        var factory = CreateHttpClientFactory(response);
        var provider = new OpenWeatherCoordinatesProvider(factory, CreateOptions("key"));

        await Assert.ThrowsAsync<DataProviderResponseException>(() => provider.GetCoordinatesAsync("London"));
    }
}
