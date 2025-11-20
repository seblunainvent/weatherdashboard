namespace WeatherDashboard.DataProviders.Abstractions.Exceptions
{
    /// <summary>
    /// Exception thrown when a requested resource is not found on the provider.
    /// Typically maps to HTTP 404 responses from external APIs.
    /// </summary>
    public class DataProviderResourceNotFoundException : DataProviderException
    {
        public DataProviderResourceNotFoundException(string message) : base(message) { }
    }
}