# Weather Dashboard

## Project Overview

Weather Dashboard is a lightweight ASP.NET Core Web API that returns current weather for user-specified locations. It is designed as a reference implementation to demonstrate clean architecture, separation of concerns, and pragmatic engineering choices for integrating external data providers.

Key design goals:

- Clear abstractions between application layers (providers, services, API controllers).
- Extensibility: providers and caches are wired via interfaces and Dependency Injection.
- Robust error handling with mapping of provider errors to user-friendly HTTP Problem Details.
- Caching strategies to reduce external calls and improve latency.

## Tech Stack

- .NET (project target: .NET 8.0 — see `WeatherDashboard.Api/WeatherDashboard.Api.csproj`). If you maintain older environments, the codebase is compatible with .NET 6/7 with minor adjustments.
- ASP.NET Core Web API
- Built-in Dependency Injection
- IHttpClientFactory for external HTTP calls
- IMemoryCache (in-memory caching) for location coordinate caching
- OpenWeatherMap API used for geocoding (location -> coordinates) and current weather data

## Core Components

- IWeatherProvider
  - Abstraction for fetching weather data. The project contains an OpenWeatherMap implementation (`OpenWeatherMapWeatherProvider`).
- ICoordinatesProvider
  - Abstraction for resolving a location name into latitude/longitude. Implemented by `OpenWeatherCoordinatesProvider` using OpenWeatherMap geocoding.
- IUserStorage
  - Simple user storage abstraction. The default project provides an in-memory implementation (`InMemoryUserStorage`) for demo and tests.
- ILocationCoordinatesCache
  - A small cache abstraction used to store resolved coordinates to avoid repeated geocoding calls. The default `MemoryLocationCoordinatesCache` uses `IMemoryCache`.
- Response caching
  - Controller endpoints are decorated with response caching (and `ResponseCaching` middleware is enabled) to reduce duplicated provider calls for identical queries.

## How to Build and Run

1. Clone the repository and navigate to the solution root:

```bash
git clone <repo-url>
cd WeatherDashboard
```

2. Configure OpenWeatherMap credentials in `appsettings.json` (or use user-secrets / environment variables). Example `appsettings.json` snippet:

```json
{
  "OpenWeatherMap": {
    "BaseApiUrl": "https://api.openweathermap.org",
    "ApiKey": "<YOUR_OPENWEATHERMAP_API_KEY>"
  }
}
```

3. Restore and run using the .NET SDK (replace `dotnet` with your desired CLI if you use a different toolchain):

```bash
dotnet restore
dotnet run --project src/be/WeatherDashboard.Api/WeatherDashboard.Api.csproj
```

Alternatively open the solution (`WeatherDashboard.sln`) in Visual Studio, Rider, or VS Code and run the `WeatherDashboard.Api` project.

4. Dependency Injection / registration

Providers, caches and middleware are registered in `Program.cs` via extension methods. Example of the registration flow used in this project:

```csharp
builder.Services.ConfigureOpenWeatherMapProvider(builder.Configuration)
    .AddOpenWeatherMapProvider()
    .AddOpenWeatherMapCoordinatesProvider();

builder.Services.AddTransient<ILocationCoordinatesCache, MemoryLocationCoordinatesCache>();
builder.Services.AddTransient<IWeatherService, WeatherService>();
builder.Services.AddTransient<IUserStorage, InMemoryUserStorage>();

builder.Services.AddMemoryCache();
builder.Services.AddResponseCaching();
```

5. Example requests

- Fetch weather by location name:

```bash
curl "https://localhost:5001/api/weather?locationName=London"
```

- Get / save a user's default location:

```bash
curl "https://localhost:5001/api/user/location?userId=123"
curl -X POST "https://localhost:5001/api/user/location?userId=123" \
  -H "Content-Type: application/json" \
  -d '{"locationName":"London"}'
```

## Error Handling

The project centralizes provider error handling and exposes user-friendly Problem Details responses.

- HTTP status codes returned by external providers are mapped to domain-specific exceptions inside provider implementations (see `OpenWeatherCoordinatesProvider` and `OpenWeatherMapWeatherProvider`):
  - 400 -> DataProviderInvalidRequestException
  - 401 -> DataProviderApiKeyException
  - 404 -> DataProviderResourceNotFoundException
  - 429 -> DataProviderResponseException (rate limit)
  - 5xx -> DataProviderResponseException

- Low-level error normalization:
  - TaskCanceledException (when not caused by caller cancellation) -> DataProviderTimeoutException
  - JsonException -> DataProviderResponseException (malformed provider response)
  - HttpRequestException -> DataProviderException

- Middleware
  - `WeatherProviderExceptionHandlingMiddleware` catches `DataProviderException` types and maps them to appropriate HTTP statuses and Problem Details payloads (for example, 503 for API key errors, 504 for timeouts, 502 for invalid provider responses, 404 for resource not found, 400 for bad requests).
  - `GlobalExceptionHandlingMiddleware` captures non-provider exceptions and returns a standardized 500 Problem Details response.

This approach ensures consistent, actionable error messages for API consumers while keeping provider-specific concerns isolated.

## Observations and Challenges

- Provider abstraction expectations
  - The current `IWeatherProvider` + `ICoordinatesProvider` flow is tailored for OpenWeatherMap, which separates geocoding and weather APIs (requires coordinates). Other providers sometimes accept location names directly — supporting those uniformly would require either extending interfaces or introducing an adapter that can accept both coordinates and names.

- User storage
  - `IUserStorage` is implemented as `InMemoryUserStorage` for demo/testing. Replace with persistent storage (e.g., SQL, Cosmos DB, Azure Table Storage) and integrate authentication for production.

- Location handling
  - If geocoding fails, the current behavior surfaces a 404. UX could be improved with autocomplete, fuzzy matching, or a fallback provider to increase hit-rate and friendliness.

- Core problems addressed by the project
  - Meaningful error mapping and middleware for predictable API responses.
  - Coordinate caching (`ILocationCoordinatesCache`) to avoid repeated geocoding calls.
  - Response caching for weather endpoints to reduce provider load.
  - Clear use of interfaces and DI to make the implementation testable and extensible.

## Where to Look in the Codebase

- `src/be/WeatherDashboard.Api/Program.cs` — DI registration and middleware.
- `src/be/WeatherDashboard.Api/Controllers/WeatherController.cs` — Weather endpoint.
- `src/be/WeatherDashboard.Api/Controllers/UserController.cs` — User default location endpoints.
- `src/be/WeatherDashboard.DataProviders.OpenWeatherMap/*` — Provider implementations and configuration.
- `src/be/WeatherDashboard.Api/Caching/MemoryLocationCoordinatesCache.cs` — in-memory coordinate cache.
- `src/be/WeatherDashboard.Core/User/InMemoryUserStorage.cs` — demo user storage.
- `src/be/WeatherDashboard.Api.Common/ErrorHandling/*` — global and provider-specific exception handling middleware.

## Unit tests

This repository includes several test projects covering controllers, services and data provider logic. Tests are written using xUnit and Moq and are placed under the `src/be` test folders.

Common test projects (folder names reflect the projects in this repo):
- src/be/WeatherDashboard.Api.Common.Tests
- src/be/WeatherDashboard.Api.Tests
- src/be/WeatherDashboard.Core.Tests
- src/be/WeatherDashboard.DataProviders.OpenWeatherMap.Tests

Run all tests (solution-level)
- From the repository root:
  - dotnet test WeatherDashboard.sln
  - This will restore, build and run all test projects in the solution.

Run a single test project
- From the repository root run:
  - dotnet test ./src/be/WeatherDashboard.Api.Tests/WeatherDashboard.Api.Tests.csproj
  - dotnet test ./src/be/WeatherDashboard.Core.Tests/WeatherDashboard.Core.Tests.csproj
  - dotnet test ./src/be/WeatherDashboard.Api.Common.Tests/WeatherDashboard.Api.Common.Tests.csproj
  - dotnet test ./src/be/WeatherDashboard.DataProviders.OpenWeatherMap.Tests/WeatherDashboard.DataProviders.OpenWeatherMap.Tests.csproj

Notes and tips
- Most unit tests mock external dependencies (IHttpClientFactory, providers, caches) so no external API keys are required.
- Provider tests that simulate HTTP responses use custom HttpMessageHandler mocks and do not perform real network calls.