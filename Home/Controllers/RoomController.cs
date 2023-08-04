
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using smart_home_server.Exceptions;
using smart_home_server.Home.Authorization;
using smart_home_server.Home.Models;
using smart_home_server.Home.ResourceModels;
using smart_home_server.Home.Services;

[ApiController]
[Route("api/v1/home/{homeId}/floor/{floorId}/[controller]")]
public class RoomController : ControllerBase
{
    private readonly IHomeService _homeService;
    private readonly IRoomService _roomService;
    private readonly IAuthorizationService _authorizationService;

    public RoomController(
        IHomeService homeService,
        IRoomService roomService,
        IAuthorizationService authorizationService)
    {
        _homeService = homeService;
        _roomService = roomService;
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

    [Authorize]
    [HttpGet]
    public async Task<ActionResult<List<Room>>> GetRooms(string homeId, string floorId, [FromQuery] SearchOptionsQuery options)
    {
        var rooms = await _roomService.GetFloorRooms(await GetHomeByParams(homeId, HomeOperation.All), floorId, options);
        return rooms;
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> PostRoom(string homeId, string floorId, [FromBody] NameDto dto)
    {
        var room = await _roomService.CreateRoom(await GetHomeByParams(homeId, HomeOperation.Installer), floorId, dto.Name);
        return CreatedAtAction(nameof(PostRoom), new { id = room.Id }, room);
    }

    [Authorize]
    [HttpPut("{roomId}")]
    public async Task<IActionResult> UpdateRoom(string homeId, string floorId, string roomId, [FromBody] UpdateRoomDto dto)
    {
        await _roomService.UpdateRoom(await GetHomeByParams(homeId, HomeOperation.Installer), floorId, roomId, dto.Name, dto.IsFavorite);
        return NoContent();
    }

    [Authorize]
    [HttpDelete("{roomId}")]
    public async Task<IActionResult> DeleteRoom(string homeId, string floorId, string roomId)
    {
        await _roomService.DeleteRoom(await GetHomeByParams(homeId, HomeOperation.Installer), floorId, roomId);
        return NoContent();
    }
}