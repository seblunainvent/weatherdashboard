namespace WeatherDashboard.Api.Abstractions.Dto
{
    public class Weather
    {
        public double TemperatureCelsius { get; set; }
        public int HumidityPercent { get; set; }
        public double WindSpeedKph { get; set; }
        public string? IconUrl { get; set; }
        public string? Description { get; set; }
    }
}