using System.Text.Json.Serialization;

namespace WeatherDashboard.DataProviders.OpenWeatherMap.Models;

public class Coord
{
    [JsonPropertyName("lon")]
    public double lon { get; set; }

    [JsonPropertyName("lat")]
    public double lat { get; set; }
}