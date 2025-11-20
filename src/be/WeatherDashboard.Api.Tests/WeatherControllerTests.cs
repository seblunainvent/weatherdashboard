using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WeatherDashboard.Api.Abstractions.Dto;
using WeatherDashboard.Core.Services;
using WeatherDashboard.DataProviders.Abstractions.Models;
using WeatherDashboard.Api.Controllers;
using Xunit;

namespace WeatherDashboard.Api.Tests;

public class WeatherControllerTests
{
    [Fact]
    public async Task Get_ReturnsOk_WithWeatherResponse()
    {
        var mockService = new Mock<IWeatherService>();
        var providerResponse = new WeatherProviderResponse
        {
            Latitude = 51.5,
            Longitude = -0.12,
            WeatherData = new WeatherData { TemperatureCelsius = 10, HumidityPercent = 80, WindSpeedKph = 5, Description = "Cloudy", IconUrl = "icon" }
        };

        mockService.Setup(s => s.GetWeatherAsync("London", It.IsAny<CancellationToken>())).ReturnsAsync(providerResponse);

        var controller = new WeatherController(mockService.Object);

        var result = await controller.Get("London", CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<WeatherResponse>(ok.Value);

        Assert.Equal("London", response.Location);
        Assert.Equal(51.5, response.Latitude);
        Assert.Equal(-0.12, response.Longitude);
        Assert.Equal(10, response.Weather.TemperatureCelsius);
        Assert.Equal(80, response.Weather.HumidityPercent);
    }
}

