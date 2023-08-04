using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using smart_home_server.Exceptions;
using smart_home_server.Home.Authorization;
using smart_home_server.Home.Models;
using smart_home_server.Home.Services;
using smart_home_server.SmartDevices.Models;
using smart_home_server.SmartDevices.ResourceModels;
using smart_home_server.SmartDevices.Services;

namespace smart_home_server.SmartDevices.Controller;

[ApiController]
[Route("/api/v1/home/{homeId}/device")]
public class SmartDeviceController : ControllerBase
{
    private readonly ISmartDeviceService _smartDeviceService;
    private readonly IHomeService _homeService;
    private readonly IRoomService _roomService;
    private readonly IAuthorizationService _authorizationService;

    public SmartDeviceController(
        ISmartDeviceService smartDeviceService,
        IHomeService homeSerivce,
        IRoomService roomSerivce,
        IAuthorizationService authorizationService
    )
    {
        _smartDeviceService = smartDeviceService;
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
    public async Task<ActionResult<List<SmartDevice>>> GetHomeDevices(
        string homeId,
        [FromQuery] string? roomId
    )
    {
        var home = await GetHomeByParams(homeId, HomeOperation.All);
        Room? room = null;
        if (roomId != null) room = await GetRoomByParams(home, roomId);
        return (await _smartDeviceService.FindDevicesInHome(home, room));
    }

    [Authorize]
    [HttpGet("count")]
    public async Task<ActionResult<List<DeviceCount>>> GetDeviceCount(
        string homeId,
        [FromQuery] string? roomId
    )
    {
        var home = await GetHomeByParams(homeId, HomeOperation.All);
        Room? room = null;
        if (roomId != null) room = await GetRoomByParams(home, roomId);
        return (await _smartDeviceService.FindDeviceCount(home, room));
    }

    [Authorize]
    [HttpPut("{deviceId}")]
    public async Task<IActionResult> PutDevice(
        string homeId,
        string deviceId,
        [FromBody] UpdateDeviceDto dto
    )
    {
        var home = await GetHomeByParams(homeId, HomeOperation.All);
        Room? room = null;
        if (dto.RoomId != null) room = await GetRoomByParams(home, dto.RoomId);
        await _smartDeviceService.UpdateDeviceName(home, deviceId, dto.Name, room);
        return NoContent();
    }

    [Authorize]
    [HttpDelete("{deviceId}")]
    public async Task<IActionResult> DeleteDevice(
        string homeId,
        string deviceId
    )
    {
        var home = await GetHomeByParams(homeId, HomeOperation.Installer);
        await _smartDeviceService.DeleteDevice(home, deviceId);
        return NoContent();
    }
}