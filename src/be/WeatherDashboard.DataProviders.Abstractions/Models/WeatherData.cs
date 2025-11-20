namespace WeatherDashboard.DataProviders.Abstractions.Models
{
    public class WeatherData
    {
        public double TemperatureCelsius { get; set; }
        public int HumidityPercent { get; set; }
        public double WindSpeedKph { get; set; }
        public string? IconUrl { get; set; }
        public string? Description { get; set; }
    }
}