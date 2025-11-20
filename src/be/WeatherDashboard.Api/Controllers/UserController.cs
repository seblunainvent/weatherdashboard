using Microsoft.AspNetCore.Mvc;
using WeatherDashboard.Core.User;

namespace WeatherDashboard.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserStorage _userStorage;

    public UserController(IUserStorage userStorage)
    {
        _userStorage = userStorage;
    }
    
    /// <summary>
    /// Gets current weather data for a given latitude/longitude.
    /// </summary>
    [HttpGet("location")]
    public async Task<ActionResult<UserLocationResponse>> GetUserLocation(
        [FromQuery] string userId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(userId))
        {
            return BadRequest();
        }

        var userLocation = await _userStorage.GetDefaultLocationAsync(userId, cancellationToken);

        var userLocationResponse = new UserLocationResponse() { LocationName = userLocation };
        
        return Ok(userLocationResponse);
    }

    [HttpPost("location")]
    public async Task<IActionResult> SaveUserLocation([FromQuery]string userId, [FromBody]SaveUserLocationRequest saveUserLocationRequest, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(saveUserLocationRequest.LocationName))
        {
            return BadRequest(ModelState);
        }

        await _userStorage.SaveDefaultLocationAsync(userId, saveUserLocationRequest.LocationName, cancellationToken);
        return Ok();
    }
}