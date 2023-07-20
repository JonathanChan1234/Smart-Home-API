using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using smart_home_server.Exceptions;
using smart_home_server.Home.Authorization;
using smart_home_server.Home.Models;
using smart_home_server.Home.Services;
using smart_home_server.Scenes.Models;
using smart_home_server.Scenes.ResourceModels;
using smart_home_server.Scenes.Services;
using smart_home_server.SmartDevices.SubDevices.Lights.Models;

namespace smart_home_server.Scenes.Controller;

[ApiController]
[Route("api/v1/home/{homeId}/scene/{sceneId}/action/light")]
public class LightActionController : ControllerBase
{
    private readonly ILogger<LightActionController> _logger;
    private readonly IHomeService _homeService;
    private readonly ISceneService _sceneService;
    private readonly IAuthorizationService _authorizationService;
    private readonly ILightActionService _lightActionService;

    public LightActionController(
        ILogger<LightActionController> logger,
        IHomeService homeService,
        ISceneService sceneService,
        IAuthorizationService authorizationService,
        ILightActionService lightActionService
    )
    {
        _logger = logger;
        _homeService = homeService;
        _sceneService = sceneService;
        _lightActionService = lightActionService;
        _authorizationService = authorizationService;
    }

    private async Task<(SmartHome, Scene)> GetHomeScene(string homeId, string sceneId)
    {
        var home = await _homeService.GetHomeById(homeId, null);
        if (home == null)
            throw new ModelNotFoundException($"Cannot find home {homeId}");

        if (!(await _authorizationService.AuthorizeAsync(User, home, HomeOperation.All)).Succeeded)
            throw new ForbiddenException($"No permisson for home {homeId}");

        var scene = await _sceneService.FindSceneById(homeId, sceneId);
        if (scene == null)
            throw new ModelNotFoundException($"Cannot find scene {sceneId}");
        return (home, scene);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> PostLightAction(
        string homeId,
        string sceneId,
        [FromBody] DeviceActionDto<LightProperties> dto
    )
    {
        var (home, scene) = await GetHomeScene(homeId, sceneId);

        var action = await _lightActionService.CreateLightAction(home, scene, dto.DeviceId, dto.DeviceProperties);
        return CreatedAtAction(nameof(PostLightAction), new { id = action.Id }, action);
    }

    [Authorize]
    [HttpPut("{actionId}")]
    public async Task<IActionResult> PutLightAction(
        string homeId,
        string sceneId,
        string actionId,
        [FromBody] DeviceActionDto<LightProperties> dto
    )
    {
        var (home, scene) = await GetHomeScene(homeId, sceneId);
        await _lightActionService.EditLightAction(home, scene, actionId, dto.DeviceProperties);
        return NoContent();
    }
}