using System;

namespace WeatherDashboard.DataProviders.Abstractions.Exceptions
{
    public class DataProviderTimeoutException : DataProviderException
    {
        public DataProviderTimeoutException(string message, Exception ex) : base(message, ex) { }
    }
}