namespace WeatherDashboard.Api.Abstractions.Dto
{
    public class WeatherResponse
    {
        public string Location { get; set; }

        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public Weather Weather { get; set; }
    }
}