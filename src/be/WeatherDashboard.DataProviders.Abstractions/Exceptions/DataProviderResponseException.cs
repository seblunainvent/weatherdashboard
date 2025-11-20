using System;

namespace WeatherDashboard.DataProviders.Abstractions.Exceptions
{
    public class DataProviderResponseException : DataProviderException
    {
        public DataProviderResponseException(string message) : base(message) { }
        public DataProviderResponseException(string message, Exception ex) : base(message, ex) { }
    }
}