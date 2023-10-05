
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using smart_home_server.Exceptions;
using smart_home_server.Home.Authorization;
using smart_home_server.Home.Models;
using smart_home_server.Home.Services;
using smart_home_server.Scenes.Models;
using smart_home_server.Scenes.ResourceModels;
using smart_home_server.Scenes.Services;
using smart_home_server.SmartDevices.SubDevices.AirConditioner.Models;

namespace smart_home_server.Scenes.Controller;

[ApiController]
[Route("api/v1/home/{homeId}/scene/{sceneId}/action/ac")]
public class AirConditionerActionController : ControllerBase
{
    private readonly ILogger<AirConditionerActionController> _logger;
    private readonly IHomeService _homeService;
    private readonly ISceneService _sceneService;
    private readonly IAuthorizationService _authorizationService;
    private readonly IAirConditionerActionService _airConditionerActionService;

    public AirConditionerActionController(
        ILogger<AirConditionerActionController> logger,
        IHomeService homeService,
        ISceneService sceneService,
        IAuthorizationService authorizationService,
        IAirConditionerActionService airConditionerActionService
    )
    {
        _logger = logger;
        _homeService = homeService;
        _sceneService = sceneService;
        _airConditionerActionService = airConditionerActionService;
        _authorizationService = authorizationService;
    }

    private async Task<(SmartHome, Scene)> GetHomeScene(string homeId, string sceneId)
    {
        var home = await _homeService.GetHomeById(homeId, null);
        if (home == null)
            throw new ModelNotFoundException($"Cannot find home {homeId}");

        if (!(await _authorizationService.AuthorizeAsync(User, home, HomeOperation.All)).Succeeded)
            throw new ForbiddenException($"No permisson for home {homeId}");

        var scene = await _sceneService.FindSceneById(homeId, sceneId)
            ?? throw new ModelNotFoundException($"Cannot find scene {sceneId}");
        return (home, scene);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> PostAirConditionerAction(
        string homeId,
        string sceneId,
        [FromBody] DeviceActionDto<AirConditionerProperties> dto
    )
    {
        var (home, scene) = await GetHomeScene(homeId, sceneId);

        var action = await _airConditionerActionService.CreateAirConditionerAction(home, scene, dto.DeviceId, dto.DeviceProperties);
        return CreatedAtAction(nameof(PostAirConditionerAction), new { id = action.Id }, action);
    }

    [Authorize]
    [HttpPut("{actionId}")]
    public async Task<IActionResult> PutAirConditionerAction(
        string homeId,
        string sceneId,
        string actionId,
        [FromBody] DeviceActionDto<AirConditionerProperties> dto
    )
    {
        var (home, scene) = await GetHomeScene(homeId, sceneId);
        await _airConditionerActionService.EditAirConditionerAction(home, scene, actionId, dto.DeviceProperties);
        return NoContent();
    }
}