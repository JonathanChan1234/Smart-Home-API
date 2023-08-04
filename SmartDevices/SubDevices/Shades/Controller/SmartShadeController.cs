using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using smart_home_server.Exceptions;
using smart_home_server.Home.Authorization;
using smart_home_server.Home.Models;
using smart_home_server.Home.Services;
using smart_home_server.SmartDevices.Models;
using smart_home_server.SmartDevices.ResourceModels;
using smart_home_server.SmartDevices.SubDevices.Shades.Models;
using smart_home_server.SmartDevices.SubDevices.Shades.Services;

namespace smart_home_server.SmartDevices.SubDevices.Shades.Controllers;

[ApiController]
[Route("api/v1/home/{homeId}/shade")]
public class SmartShadeController : ControllerBase
{
    private readonly ISmartShadeService _smartShadeService;
    private readonly IHomeService _homeService;
    private readonly IRoomService _roomService;
    private readonly IAuthorizationService _authorizationService;

    public SmartShadeController(
        ISmartShadeService smartShadeService,
        IHomeService homeSerivce,
        IRoomService roomSerivce,
        IAuthorizationService authorizationService
    )
    {
        _smartShadeService = smartShadeService;
        _homeService = homeSerivce;
        _roomService = roomSerivce;
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

    private async Task<Room> GetRoomByParams(SmartHome home, string roomId)
    {
        var room = await _roomService.GetRoomInHome(home, roomId);
        if (room == null)
            throw new ModelNotFoundException($"Cannot find room {roomId}");
        return room;
    }

    [Authorize]
    [HttpGet]
    public async Task<ActionResult<List<SmartDevice>>> GetAllShades(
        string homeId,
        [FromQuery] string? roomId
    )
    {
        var home = await GetHomeByParams(homeId, HomeOperation.All);
        Room? room = null;
        if (roomId != null) room = await GetRoomByParams(home, roomId);
        return (await _smartShadeService.FindShadeInHome(home, room));
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<SmartDevice>> PostShade(
        string homeId,
        [FromBody] CreateDeviceDto<ShadeCapabilities> dto
    )
    {
        var home = await GetHomeByParams(homeId, HomeOperation.Installer);
        var room = await GetRoomByParams(home, dto.RoomId);

        var shade = await _smartShadeService.CreateShadeDevice(
            home,
            room,
            dto.Name,
            dto.SubCategory,
            dto.Capabilities
        );
        return CreatedAtAction(nameof(PostShade), new { id = shade.Id }, shade);
    }

    [Authorize]
    [HttpPut("{deviceId}/status")]
    public async Task<IActionResult> UpdateShadeStatus(
        string homeId,
        string deviceId,
        [FromBody] UpdateDeviceStatusDto<ShadeProperties> dto
    )
    {
        var home = await GetHomeByParams(homeId, HomeOperation.Installer);
        await _smartShadeService.UpdateShadeStatus(home, deviceId, dto.LastUpdatedAt, dto.Properties, dto.OnlineStatus);
        return NoContent();
    }

    [Authorize]
    [HttpPut("{deviceId}/capabilities")]
    public async Task<IActionResult> UpdateLightCapabilites(
        string homeId,
        string deviceId,
        [FromBody] ShadeCapabilities capabilities
    )
    {
        var home = await GetHomeByParams(homeId, HomeOperation.Installer);
        await _smartShadeService.UpdateShadeCapabilities(home, deviceId, capabilities);
        return NoContent();
    }
}