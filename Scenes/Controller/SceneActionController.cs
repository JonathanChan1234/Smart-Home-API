using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using smart_home_server.Exceptions;
using smart_home_server.Home.Authorization;
using smart_home_server.Home.Models;
using smart_home_server.Home.Services;
using smart_home_server.Scenes.Models;
using smart_home_server.Scenes.Services;
using smart_home_server.SmartDevices.Models;

[ApiController]
[Route("api/v1/home/{homeId}/scene/{sceneId}/action")]
public class SceneActionController : ControllerBase
{
    private readonly ILogger<SceneActionController> _logger;
    private readonly IHomeService _homeService;
    private readonly ISceneService _sceneService;
    private readonly IAuthorizationService _authorizationService;
    private readonly ISceneActionService _sceneActionService;

    public SceneActionController(
        ILogger<SceneActionController> logger,
        IHomeService homeService,
        ISceneService sceneService,
        IAuthorizationService authorizationService,
        ISceneActionService sceneActionService
    )
    {
        _logger = logger;
        _homeService = homeService;
        _sceneService = sceneService;
        _sceneActionService = sceneActionService;
        _authorizationService = authorizationService;
    }

    private async Task<(SmartHome, Scene)> GetHomeScene(string homeId, string sceneId)
    {
        var home = await _homeService.GetHomeById(homeId, null);
        if (home == null)
            throw new ModelNotFoundException($"cannot find home id {homeId}");

        if (!(await _authorizationService.AuthorizeAsync(User, home, HomeOperation.All)).Succeeded)
            throw new ForbiddenException("you do have permission to this home");

        var scene = await _sceneService.FindSceneById(homeId, sceneId);
        if (scene == null) throw new ModelNotFoundException($"Cannot find scene id {sceneId}");
        return (home, scene);
    }

    [Authorize]
    [HttpGet]
    public async Task<ActionResult<List<SceneAction>>> GetSceneActions(
        string homeId,
        string sceneId
    )
    {
        var (home, scene) = await GetHomeScene(homeId, sceneId);
        return (await _sceneActionService.FindSceneActions(sceneId));
    }

    [Authorize]
    [HttpGet("devices")]
    public async Task<ActionResult<List<SmartDevice>>> GetAvailableDevices(
        string homeId,
        string sceneId
    )
    {
        var (home, scene) = await GetHomeScene(homeId, sceneId);
        return (await _sceneActionService.FindAvailableSmartDevices(home, scene));
    }

    [Authorize]
    [HttpDelete("{actionId}")]
    public async Task<IActionResult> DeleteSceneAction(
        string homeId,
        string sceneId,
        string actionId)
    {
        var (home, scene) = await GetHomeScene(homeId, sceneId);
        await _sceneActionService.DeleteSceneAction(scene, actionId);
        return NoContent();
    }
}