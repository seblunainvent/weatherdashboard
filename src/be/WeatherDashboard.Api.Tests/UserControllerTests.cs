using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WeatherDashboard.Api.Abstractions.Dto;
using WeatherDashboard.Core.User;
using WeatherDashboard.Api.Controllers;
using Xunit;

namespace WeatherDashboard.Api.Tests;

public class UserControllerTests
{
    [Fact]
    public async Task GetUserLocation_ReturnsBadRequest_WhenUserIdMissing()
    {
        var mock = new Mock<IUserStorage>();
        var controller = new UserController(mock.Object);

        var result = await controller.GetUserLocation("", CancellationToken.None);

        Assert.IsType<BadRequestResult>(result.Result);
    }

    [Fact]
    public async Task GetUserLocation_ReturnsLocation_WhenFound()
    {
        var mock = new Mock<IUserStorage>();
        mock.Setup(m => m.GetDefaultLocationAsync("user1", It.IsAny<CancellationToken>())).ReturnsAsync("London");

        var controller = new UserController(mock.Object);

        var result = await controller.GetUserLocation("user1", CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var payload = Assert.IsType<UserLocationResponse>(ok.Value);
        Assert.Equal("London", payload.LocationName);
    }

    [Fact]
    public async Task SaveUserLocation_ReturnsBadRequest_WhenInvalid()
    {
        var mock = new Mock<IUserStorage>();
        var controller = new UserController(mock.Object);

        var request = new SaveUserLocationRequest { LocationName = "" };
        var result = await controller.SaveUserLocation("", request, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task SaveUserLocation_CallsStorageAndReturnsOk()
    {
        var mock = new Mock<IUserStorage>();
        mock.Setup(m => m.SaveDefaultLocationAsync("user1", "Paris", It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var controller = new UserController(mock.Object);
        var request = new SaveUserLocationRequest { LocationName = "Paris" };

        var result = await controller.SaveUserLocation("user1", request, CancellationToken.None);

        Assert.IsType<OkResult>(result);
        mock.Verify(m => m.SaveDefaultLocationAsync("user1", "Paris", It.IsAny<CancellationToken>()), Times.Once);
    }
}

