namespace WeatherDashboard.DataProviders.Abstractions.Models
{
    public class WeatherProviderResponse
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public WeatherData WeatherData { get; set; }
    }
}