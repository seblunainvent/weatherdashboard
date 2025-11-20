using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using WeatherDashboard.DataProviders.Abstractions;
using WeatherDashboard.DataProviders.Abstractions.Exceptions;
using WeatherDashboard.DataProviders.Abstractions.Models;
using WeatherDashboard.DataProviders.OpenWeatherMap.Models;

namespace WeatherDashboard.DataProviders.OpenWeatherMap;

public class OpenWeatherCoordinatesProvider : ICoordinatesProvider
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly OpenWeatherMapDataProviderConfiguration _config;

    public OpenWeatherCoordinatesProvider(
        IHttpClientFactory httpClientFactory,
        IOptions<OpenWeatherMapDataProviderConfiguration> options)
    {
        _httpClientFactory = httpClientFactory;
        _config = options.Value ?? throw new DataProviderApiKeyException("OpenWeather configuration is missing.");

        if (string.IsNullOrWhiteSpace(_config.ApiKey))
            throw new DataProviderApiKeyException("OpenWeather API key is missing.");
    }

    public async Task<Coordinates> GetCoordinatesAsync(
        string locationName,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(locationName))
            throw new DataProviderInvalidRequestException("Location name must be provided.");

        var url = BuildRequestUri(locationName);

        var httpClient = _httpClientFactory.CreateClient(Constants.OpenWeatherMapHttpClientName);

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
                        throw new DataProviderResourceNotFoundException($"Location '{locationName}' not found.");
                    case (HttpStatusCode)429:             // Too Many Requests
                        throw new DataProviderResponseException("Rate limit exceeded on OpenWeatherMap API.");
                    default:
                        if ((int)response.StatusCode >= 500)
                            throw new DataProviderResponseException($"OpenWeatherMap server error: {response.StatusCode}");
                        throw new DataProviderResponseException($"Unexpected response: {response.StatusCode}");
                }
            }

            var results = await response.Content.ReadFromJsonAsync<OpenWeatherGeoResponse[]>(cancellationToken);

            if (results == null || results.Length == 0)
                throw new DataProviderResourceNotFoundException($"No coordinates found for location '{locationName}'.");

            var first = results[0];
            return new Coordinates() { Latitude = first.Lat,  Longitude = first.Lon };
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

    private string BuildRequestUri(string locationName)
    {
        var queryParams = new Dictionary<string, string?>
        {
            ["q"] = locationName,
            ["limit"] = "1",
            ["appid"] = _config.ApiKey,
        };

        var query = string.Join("&", queryParams
            .Where(kvp => kvp.Value != null)
            .Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));

        return $"/geo/1.0/direct?{query}";
    }
}