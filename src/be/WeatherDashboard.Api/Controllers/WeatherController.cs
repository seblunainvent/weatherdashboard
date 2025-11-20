using Microsoft.AspNetCore.Mvc;
using WeatherDashboard.Api.Abstractions.Dto;
using WeatherDashboard.Core.Services;
using WeatherDashboard.DataProviders.Abstractions;
using WeatherDashboard.DataProviders.Abstractions.Exceptions;
using WeatherDashboard.DataProviders.Abstractions.Models;

namespace WeatherDashboard.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WeatherController : ControllerBase
{
    private readonly IWeatherService _weatherService;

    public WeatherController(IWeatherService weatherService)
    {
        _weatherService = weatherService;
    }

    /// <summary>
    /// Gets current weather data for a given latitude/longitude.
    /// </summary>
    [HttpGet]
    [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any, VaryByQueryKeys = ["locationName"])]
    public async Task<ActionResult<WeatherProviderResponse>> Get(
        [FromQuery] string locationName,
        CancellationToken cancellationToken)
    {
        var result = await _weatherService.GetWeatherAsync(locationName, cancellationToken);

        var weatherResponse = new WeatherResponse()
        {
            Location = locationName,
            Latitude = result.Latitude,
            Longitude = result.Longitude,
            Weather = new Weather()
            {
                HumidityPercent = result.WeatherData.HumidityPercent,
                TemperatureCelsius = result.WeatherData.TemperatureCelsius,
                WindSpeedKph = result.WeatherData.WindSpeedKph,
                IconUrl =  result.WeatherData.IconUrl,
                Description = result.WeatherData.Description
            }
        };
        
        return Ok(weatherResponse);
    }
}