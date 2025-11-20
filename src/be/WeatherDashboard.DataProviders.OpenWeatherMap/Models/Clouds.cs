using System.Text.Json.Serialization;

namespace WeatherDashboard.DataProviders.OpenWeatherMap.Models;

    public class Clouds
    {
        [JsonPropertyName("all")]
        public int all { get; set; }
    }