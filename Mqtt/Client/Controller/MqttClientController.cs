using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using smart_home_server.Auth.Exceptions;
using smart_home_server.Auth.Models;
using smart_home_server.Auth.Services;
using smart_home_server.Exceptions;
using smart_home_server.Home.Authorization;
using smart_home_server.Home.Models;
using smart_home_server.Home.Services;
using smart_home_server.Mqtt.Client.Models;
using smart_home_server.Mqtt.Client.Services;

namespace smart_home_server.Mqtt.Client.Controller;

[ApiController]
[Route("api/v1/home/{homeId}/[controller]")]
public class MqttClientController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IHomeService _homeService;
    private readonly IAuthorizationService _authorizationService;
    private readonly IMqttClientService _mqttClientService;

    public MqttClientController(
        IAuthService authService,
        IHomeService homeService,
        IAuthorizationService authorizationService,
        IMqttClientService mqttClientService)
    {
        _authService = authService;
        _homeService = homeService;
        _authorizationService = authorizationService;
        _mqttClientService = mqttClientService;
    }

    private async Task<ApplicationUser> GetAuthUser()
    {
        var userId = HttpContext.User.FindFirst(JwtService.USER_ID)?.Value;
        if (userId == null) throw new BadAuthenticationException("Empty User ID");
        var user = await _authService.GetUserById(userId);
        if (user == null) throw new BadAuthenticationException("Non existing user");
        return user;
    }

    private async Task<SmartHome> GetHomeByParams(string homeId, HomeOperationRequirement right)
    {
        var home = await _homeService.GetHomeById(homeId, null);
        if (home == null)
            throw new ModelNotFoundException($"Cannot find home {homeId}");

        if (!(await _authorizationService.AuthorizeAsync(User, home, right)).Succeeded)
            throw new ForbiddenException($"No permisson for home {homeId}");
        return home;
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<MqttClient>> CreateMqttClient(string homeId)
    {
        var home = await GetHomeByParams(homeId, HomeOperation.All);
        var client = await _mqttClientService.RegisterMqttClient(await GetAuthUser(), home);
        return client;
    }

    [Authorize]
    [HttpDelete]
    public async Task<IActionResult> RevokeMqttClient(string homeId)
    {
        var home = await GetHomeByParams(homeId, HomeOperation.All);
        await _mqttClientService.RevokeAllMqttClients(await GetAuthUser(), home);
        return NoContent();
    }
}