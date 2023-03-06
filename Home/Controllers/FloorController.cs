using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using smart_home_server.Auth.Exceptions;
using smart_home_server.Auth.Models;
using smart_home_server.Auth.Services;
using smart_home_server.Home.Models;
using smart_home_server.Home.ResourceModels;

[ApiController]
[Route("api/v1/Home/{homeId}/[controller]")]
public class FloorController : ControllerBase
{
    private readonly IFloorService _floorService;
    private readonly IAuthService _authService;
    public FloorController(IFloorService floorService, IAuthService authService)
    {
        _floorService = floorService;
        _authService = authService;
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
        var floors = await _floorService.GetHomeFloors((await GetAuthUser()).Id, homeId, options);
        return floors;
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> PostFloor(string homeId, [FromBody] NameDto dto)
    {
        var floor = await _floorService.CreateFloor((await GetAuthUser()).Id, homeId, dto.Name);
        return CreatedAtAction(nameof(PostFloor), new { id = floor.Id }, floor);
    }

    [Authorize]
    [HttpPut("{floorId}")]
    public async Task<IActionResult> UpdateFloor(string homeId, string floorId, [FromBody] NameDto dto)
    {
        await _floorService.UpdateFloor((await GetAuthUser()).Id, homeId, floorId, dto.Name);
        return NoContent();
    }

    [Authorize]
    [HttpDelete("{floorId}")]
    public async Task<IActionResult> DeleteFloor(string homeId, string floorId)
    {
        await _floorService.DeleteFloor((await GetAuthUser()).Id, homeId, floorId);
        return NoContent();
    }
}