
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using smart_home_server.Exceptions;
using smart_home_server.Home.Authorization;
using smart_home_server.Home.Models;
using smart_home_server.Home.Services;
using smart_home_server.SmartDevices.Models;
using smart_home_server.SmartDevices.ResourceModels;
using smart_home_server.SmartDevices.SubDevices.AirConditioner.Models;
using smart_home_server.SmartDevices.SubDevices.AirConditioner.Service;

namespace smart_home_server.SmartDevices.SubDevices.AirConditioner.Controller;

[ApiController]
[Route("/api/v1/home/{homeId}/ac")]
public class AirConditionerController : ControllerBase
{
    private readonly IAirConditionerService _airConditionerService;
    private readonly IHomeService _homeService;
    private readonly IRoomService _roomService;
    private readonly IAuthorizationService _authorizationService;

    public AirConditionerController(
        IAirConditionerService airConditionerService,
        IHomeService homeSerivce,
        IRoomService roomSerivce,
        IAuthorizationService authorizationService
    )
    {
        _airConditionerService = airConditionerService;
        _homeService = homeSerivce;
        _roomService = roomSerivce;
        _authorizationService = authorizationService;
    }

    private async Task<SmartHome> GetHomeByParams(string homeId, HomeOperationRequirement right)
    {
        var home = await _homeService.GetHomeById(homeId, null);
        if (home != null)
        {
            if (!(await _authorizationService.AuthorizeAsync(User, home, right)).Succeeded)
                throw new ForbiddenException($"No permisson for home {homeId}");
            return home;
        }
        throw new ModelNotFoundException($"Cannot find home {homeId}");
    }

    private async Task<Room> GetRoomByParams(SmartHome home, string roomId)
    {
        var room = await _roomService.GetRoomInHome(home, roomId)
            ?? throw new ModelNotFoundException($"Cannot find room {roomId}");
        return room;
    }

    [Authorize]
    [HttpGet]
    public async Task<ActionResult<List<SmartDevice>>> GetAirConditioner(
        string homeId,
        [FromQuery] string? roomId
    )
    {
        var home = await GetHomeByParams(homeId, HomeOperation.All);
        Room? room = null;
        if (roomId != null) room = await GetRoomByParams(home, roomId);
        return await _airConditionerService.FindAirConditionerInHome(home, room);
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<SmartDevice>> PostAirConditioner(
        string homeId,
        [FromBody] CreateDeviceDto<AirConditionerCapabilities> dto
    )
    {
        var home = await GetHomeByParams(homeId, HomeOperation.Installer);
        var room = await GetRoomByParams(home, dto.RoomId);

        var device = await _airConditionerService.CreateAirConditioner(
            home,
            room,
            dto.Name,
            dto.Capabilities
        );
        return CreatedAtAction(nameof(PostAirConditioner), new { device.Id }, device);
    }
}