using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using smart_home_server.Home.Authorization;
using smart_home_server.Home.Services;
using smart_home_server.Scenes.Models;
using smart_home_server.Scenes.ResourceModels;
using smart_home_server.Scenes.Services;

namespace smart_home_server.Scenes.Controller;

[ApiController]
[Route("api/v1/home/{homeId}/scene")]
public class SceneController : ControllerBase
{
    private readonly ILogger<SceneController> _logger;
    private readonly ISceneService _sceneService;
    private readonly IHomeService _homeService;
    private readonly IAuthorizationService _authorizationService;

    public SceneController(
        ILogger<SceneController> logger,
        ISceneService sceneService,
        IHomeService homeService,
        IAuthorizationService authorizationService
    )
    {
        _logger = logger;
        _sceneService = sceneService;
        _homeService = homeService;
        _authorizationService = authorizationService;
    }

    [Authorize]
    [HttpGet("{sceneId}")]
    public async Task<ActionResult<Scene>> GetSceneById(string homeId, string sceneId)
    {
        var home = await _homeService.GetHomeById(homeId, null);
        if (home == null)
        {
            return NotFound();
        }
        if ((await _authorizationService.AuthorizeAsync(User, home, HomeOperation.All)).Succeeded)
        {
            var scene = await _sceneService.FindSceneById(homeId, sceneId);
            if (scene == null) return NotFound();
            return scene;
        }
        return Unauthorized();
    }

    [Authorize]
    [HttpGet]
    public async Task<ActionResult<List<Scene>>> GetScenes(string homeId)
    {
        var home = await _homeService.GetHomeById(homeId, null);
        if (home == null)
        {
            return NotFound();
        }
        if ((await _authorizationService.AuthorizeAsync(User, home, HomeOperation.All)).Succeeded)
        {
            var scenes = await _sceneService.GetHomeScenes(homeId);
            return scenes;
        }
        return Unauthorized();
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> PostScene(string homeId, [FromBody] SceneDto dto)
    {
        var home = await _homeService.GetHomeById(homeId, null);
        if (home == null)
        {
            return NotFound();
        }
        if ((await _authorizationService.AuthorizeAsync(User, home, HomeOperation.All)).Succeeded)
        {
            var scene = await _sceneService.CreateScene(homeId, dto.Name);
            return CreatedAtAction(nameof(PostScene), new { id = scene.Id }, scene);
        }
        return Unauthorized();
    }

    [Authorize]
    [HttpPut("{sceneId}")]
    public async Task<IActionResult> PutScene(string homeId, string sceneId, [FromBody] SceneDto dto)
    {
        var home = await _homeService.GetHomeById(homeId, null);
        if (home == null)
        {
            return NotFound();
        }
        if ((await _authorizationService.AuthorizeAsync(User, home, HomeOperation.All)).Succeeded)
        {
            var scene = await _sceneService.EditSceneName(homeId, sceneId, dto.Name);
            return NoContent();
        }
        return Unauthorized();
    }

    [Authorize]
    [HttpDelete("{sceneId}")]
    public async Task<IActionResult> DeleteScene(string homeId, string sceneId)
    {
        var home = await _homeService.GetHomeById(homeId, null);
        if (home == null)
        {
            return NotFound();
        }
        if ((await _authorizationService.AuthorizeAsync(User, home, HomeOperation.All)).Succeeded)
        {
            await _sceneService.DeleteScene(homeId, sceneId);
            return NoContent();
        }
        return Unauthorized();
    }
}