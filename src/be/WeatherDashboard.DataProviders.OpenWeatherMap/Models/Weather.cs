using System.Text.Json.Serialization;

namespace WeatherDashboard.DataProviders.OpenWeatherMap.Models;

public class Weather
{
    [JsonPropertyName("id")]
    public int id { get; set; }

    [JsonPropertyName("main")]
    public string main { get; set; }

    [JsonPropertyName("description")]
    public string description { get; set; }

    [JsonPropertyName("icon")]
    public string icon { get; set; }
}