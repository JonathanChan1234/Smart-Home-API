using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using smart_home_server.Auth.Exceptions;
using smart_home_server.Auth.Models;
using smart_home_server.Auth.Services;
using smart_home_server.Exceptions;
using smart_home_server.Home.Authorization;
using smart_home_server.Home.Models;
using smart_home_server.Home.ResourceModels;
using smart_home_server.Home.Services;

[ApiController]
[Route("api/v1/Home/{homeId}/[controller]")]
public class FloorController : ControllerBase
{
    private readonly IHomeService _homeService;
    private readonly IFloorService _floorService;
    private readonly IAuthService _authService;
    private readonly IAuthorizationService _authorizationService;

    public FloorController(
        IHomeService homeService,
        IFloorService floorService,
        IAuthService authService,
        IAuthorizationService authorizationService)
    {
        _homeService = homeService;
        _floorService = floorService;
        _authService = authService;
        _authorizationService = authorizationService;
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

    private async Task<ApplicationUser> GetAuthUser()
    {
        var user = await _authService.GetUserById(User.FindFirst(JwtService.USER_ID)?.Value ?? "");
        if (user == null) throw new BadAuthenticationException();
        return user;
    }

    [Authorize]
    [HttpGet]
    public async Task<ActionResult<List<Floor>>> GetFloors(string homeId, [FromQuery] SearchOptionsQuery options)
    {
        var floors = await _floorService.GetHomeFloors(await GetHomeByParams(homeId, HomeOperation.All), options);
        return floors;
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> PostFloor(string homeId, [FromBody] NameDto dto)
    {
        var floor = await _floorService.CreateFloor(await GetHomeByParams(homeId, HomeOperation.Installer), dto.Name);
        return CreatedAtAction(nameof(PostFloor), new { id = floor.Id }, floor);
    }

    [Authorize]
    [HttpPut("{floorId}")]
    public async Task<IActionResult> UpdateFloor(string homeId, string floorId, [FromBody] NameDto dto)
    {
        await _floorService.UpdateFloor(await GetHomeByParams(homeId, HomeOperation.All), floorId, dto.Name);
        return NoContent();
    }

    [Authorize]
    [HttpDelete("{floorId}")]
    public async Task<IActionResult> DeleteFloor(string homeId, string floorId)
    {
        await _floorService.DeleteFloor(await GetHomeByParams(homeId, HomeOperation.Installer), floorId);
        return NoContent();
    }
}