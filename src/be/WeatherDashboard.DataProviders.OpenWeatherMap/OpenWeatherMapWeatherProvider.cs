using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WeatherDashboard.DataProviders.Abstractions;
using WeatherDashboard.DataProviders.Abstractions.Exceptions;
using WeatherDashboard.DataProviders.Abstractions.Models;
using WeatherDashboard.DataProviders.OpenWeatherMap.Models;

namespace WeatherDashboard.DataProviders.OpenWeatherMap;

    public class OpenWeatherMapWeatherProvider : IWeatherProvider
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly OpenWeatherMapDataProviderConfiguration _config;

        public OpenWeatherMapWeatherProvider(
            IHttpClientFactory  httpClientFactory,
            IOptions<OpenWeatherMapDataProviderConfiguration> options)
        {
            _httpClientFactory = httpClientFactory;
            _config = options.Value ?? throw new DataProviderApiKeyException("OpenWeather configuration is missing.");

            if (string.IsNullOrWhiteSpace(_config.ApiKey))
                throw new DataProviderApiKeyException("OpenWeather API key is missing.");
        }

        public async Task<WeatherProviderResponse> GetWeatherAsync(
            WeatherProviderRequest request,
            CancellationToken cancellationToken = default)
        {
            var httpClient = _httpClientFactory.CreateClient(Constants.OpenWeatherMapHttpClientName);
            
            if (request.Latitude < -90 || request.Latitude > 90 ||
                request.Longitude < -180 || request.Longitude > 180)
            {
                throw new DataProviderInvalidRequestException("Invalid latitude/longitude values.");
            }

            var url = BuildRequestUri(request.Latitude, request.Longitude);

            try
            {
                var response = await httpClient.GetAsync(url, cancellationToken);
                
                if (!response.IsSuccessStatusCode)
                {
                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.BadRequest:       // 400
                            throw new DataProviderInvalidRequestException("Bad request sent to OpenWeather.");
                        case HttpStatusCode.Unauthorized:     // 401
                            throw new DataProviderApiKeyException("Invalid or missing OpenWeather API key.");
                        case HttpStatusCode.NotFound:         // 404
                            throw new DataProviderResourceNotFoundException("Requested resource not found.");
                        case (HttpStatusCode)429:             // Too Many Requests
                            throw new DataProviderResponseException("Rate limit exceeded on OpenWeatherMap API.");
                        default:
                            if ((int)response.StatusCode >= 500)
                                throw new DataProviderResponseException($"OpenWeatherMap server error: {response.StatusCode}");
                            throw new DataProviderResponseException($"Unexpected response: {response.StatusCode}");
                    }
                }
                
                var root = await response.Content.ReadFromJsonAsync<Root>(cancellationToken);

                if (root?.main == null)
                    throw new DataProviderResponseException("OpenWeather returned an empty or malformed response.");
                
                var weatherData = new WeatherData
                {
                    TemperatureCelsius = root.main.temp,
                    HumidityPercent = root.main.humidity,
                    WindSpeedKph = root.wind.speed
                };

                var weather = root.weather.FirstOrDefault();
                if (weather != null)
                {
                    weatherData.IconUrl = $"https://openweathermap.org/img/wn/{weather.icon}@2x.png";
                    weatherData.Description = weather.description;
                }

                return new WeatherProviderResponse
                {
                    Latitude = root.coord.lat,
                    Longitude = root.coord.lon,
                    WeatherData = weatherData
                };
            }
            catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
            {
                throw new DataProviderTimeoutException("OpenWeatherMap did not respond in time.", ex);
            }
            catch (JsonException ex)
            {
                throw new DataProviderResponseException("Malformed response from OpenWeatherMap.", ex);
            }
            catch (HttpRequestException ex)
            {
                throw new DataProviderException("Error communicating with OpenWeatherMap.", ex);
            }
        }

        private string BuildRequestUri(double latitude, double longitude)
        {
            var queryParams = new Dictionary<string, string?>
            {
                ["lat"] = latitude.ToString(CultureInfo.InvariantCulture),
                ["lon"] = longitude.ToString(CultureInfo.InvariantCulture),
                ["units"] = "metric",
                ["exclude"] = "minutely,hourly,daily,alerts",
                ["appid"] = _config.ApiKey,
            };

            var query = string.Join("&", queryParams
                .Where(kvp => kvp.Value != null)
                .Select(kvp => $"{kvp.Key}={kvp.Value}"));

            return $"data/2.5/weather?{query}";
        }
    }