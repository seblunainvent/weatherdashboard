namespace WeatherDashboard.DataProviders.Abstractions.Exceptions
{
    public class DataProviderApiKeyException : DataProviderException
    {
        public DataProviderApiKeyException(string message) : base(message) { }
    }
}