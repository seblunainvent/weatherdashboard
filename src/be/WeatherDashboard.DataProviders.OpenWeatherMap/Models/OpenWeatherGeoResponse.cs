using System.Text.Json.Serialization;

namespace WeatherDashboard.DataProviders.OpenWeatherMap.Models;

internal class OpenWeatherGeoResponse
{
    [JsonPropertyName("lat")]
    public double Lat { get; set; }
    [JsonPropertyName("lon")]
    public double Lon { get; set; }
}