using System.Text.Json.Serialization;

namespace WeatherDashboard.DataProviders.OpenWeatherMap.Models;

public class Root
{
    [JsonPropertyName("coord")]
    public Coord coord { get; set; }

    [JsonPropertyName("weather")]
    public List<Weather> weather { get; set; }

    [JsonPropertyName("base")]
    public string @base { get; set; }

    [JsonPropertyName("main")]
    public Main main { get; set; }

    [JsonPropertyName("visibility")]
    public int visibility { get; set; }

    [JsonPropertyName("wind")]
    public Wind wind { get; set; }

    [JsonPropertyName("rain")]
    public Rain rain { get; set; }

    [JsonPropertyName("clouds")]
    public Clouds clouds { get; set; }

    [JsonPropertyName("dt")]
    public int dt { get; set; }

    [JsonPropertyName("sys")]
    public Sys sys { get; set; }

    [JsonPropertyName("timezone")]
    public int timezone { get; set; }

    [JsonPropertyName("id")]
    public int id { get; set; }

    [JsonPropertyName("name")]
    public string name { get; set; }

    [JsonPropertyName("cod")]
    public int cod { get; set; }
}