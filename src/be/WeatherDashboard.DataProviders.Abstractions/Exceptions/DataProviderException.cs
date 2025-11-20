using System;

namespace WeatherDashboard.DataProviders.Abstractions.Exceptions
{
    public class DataProviderException : Exception
    {
        public DataProviderException(string message) : base(message) { }
        public DataProviderException(string message, Exception innerException) : base(message, innerException) { }
    }

    // Thrown when API key is missing, invalid, or rejected by the provider

    // Thrown when the provider does not respond within the configured timeout

    // Thrown when the provider returns a malformed or unexpected response

    // Thrown when the request itself is invalid (e.g., bad lat/lon values)
}