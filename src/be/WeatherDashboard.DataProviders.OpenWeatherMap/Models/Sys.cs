using System.Text.Json.Serialization;

namespace WeatherDashboard.DataProviders.OpenWeatherMap.Models;

public class Sys
{
    [JsonPropertyName("type")]
    public int type { get; set; }

    [JsonPropertyName("id")]
    public int id { get; set; }

    [JsonPropertyName("country")]
    public string country { get; set; }

    [JsonPropertyName("sunrise")]
    public int sunrise { get; set; }

    [JsonPropertyName("sunset")]
    public int sunset { get; set; }
}