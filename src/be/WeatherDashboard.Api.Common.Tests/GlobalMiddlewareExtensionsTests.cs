using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using WeatherDashboard.Api.Common.ErrorHandling;
using WeatherDashboard.Api.Common.ErrorHandling.WeatherDashboard.Api.Common.Extensions;
using Xunit;

namespace WeatherDashboard.Api.Common.Tests;

public class GlobalMiddlewareExtensionsTests
{
    [Fact]
    public void AddGlobalExceptionHandling_RegistersMiddlewareAsTransient()
    {
        var services = new ServiceCollection();
        services.AddGlobalExceptionHandling();

        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(GlobalExceptionHandlingMiddleware));
        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Transient, descriptor.Lifetime);
    }
}
