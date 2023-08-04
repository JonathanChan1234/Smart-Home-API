using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using smart_home_server.Exceptions;
using smart_home_server.Home.Authorization;
using smart_home_server.Home.Models;
using smart_home_server.Home.Services;
using smart_home_server.SmartDevices.Models;
using smart_home_server.SmartDevices.ResourceModels;
using smart_home_server.SmartDevices.SubDevices.Lights.Models;
using smart_home_server.SmartDevices.SubDevices.Lights.Service;

namespace smart_home_server.SmartDevices.SubDevices.Lights.Controller;

[ApiController]
[Route("/api/v1/home/{homeId}/light")]
public class SmartLightController : ControllerBase
{
    private readonly ISmartLightService _smartLightSerivce;
    private readonly IHomeService _homeService;
    private readonly IRoomService _roomService;
    private readonly IAuthorizationService _authorizationService;

    public SmartLightController(
        ISmartLightService smartLightService,
        IHomeService homeSerivce,
        IRoomService roomSerivce,
        IAuthorizationService authorizationService
    )
    {
        _smartLightSerivce = smartLightService;
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
    public async Task<ActionResult<List<SmartDevice>>> GetLights(
        string homeId,
        [FromQuery] string? roomId
    )
    {
        var home = await GetHomeByParams(homeId, HomeOperation.All);
        Room? room = null;
        if (roomId != null) room = await GetRoomByParams(home, roomId);
        return (await _smartLightSerivce.FindLightsInHome(home, room));
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<SmartDevice>> PostLight(
        string homeId,
        [FromBody] CreateDeviceDto<LightCapabilities> dto
    )
    {
        var home = await GetHomeByParams(homeId, HomeOperation.Installer);
        var room = await GetRoomByParams(home, dto.RoomId);

        var device = await _smartLightSerivce.CreateLightDevice(
            home,
            room,
            dto.Name,
            dto.SubCategory,
            dto.Capabilities
        );
        return CreatedAtAction(nameof(PostLight), new { Id = device.Id }, device);
    }

    [Authorize]
    [HttpPut("{deviceId}/status")]
    public async Task<IActionResult> UpdateLightStatus(
        string homeId,
        string deviceId,
        [FromBody] UpdateDeviceStatusDto<LightProperties> dto
    )
    {
        var home = await GetHomeByParams(homeId, HomeOperation.Installer);
        await _smartLightSerivce.UpdateLightStatus(home, deviceId, dto.LastUpdatedAt, dto.Properties, dto.OnlineStatus);
        return NoContent();
    }

    [Authorize]
    [HttpPut("{deviceId}/capabilities")]
    public async Task<IActionResult> UpdateLightCapabilites(
        string homeId,
        string deviceId,
        [FromBody] LightCapabilities capabilities
    )
    {
        var home = await GetHomeByParams(homeId, HomeOperation.All);
        await _smartLightSerivce.UpdateLightCapabilities(home, deviceId, capabilities);
        return NoContent();
    }
}