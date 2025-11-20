namespace WeatherDashboard.DataProviders.Abstractions.Exceptions
{
    public class DataProviderInvalidRequestException : DataProviderException
    {
        public DataProviderInvalidRequestException(string message) : base(message) { }
    }
}