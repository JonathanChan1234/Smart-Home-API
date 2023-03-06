
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using smart_home_server.Auth.Exceptions;
using smart_home_server.Auth.Models;
using smart_home_server.Auth.Services;
using smart_home_server.Home.Models;
using smart_home_server.Home.ResourceModels;
using smart_home_server.Home.Services;

[ApiController]
[Route("api/v1/home/{homeId}/floor/{floorId}/[controller]")]
public class RoomController : ControllerBase
{
    private readonly IRoomService _roomService;
    private readonly IAuthService _authService;
    public RoomController(IRoomService floorService, IAuthService authService)
    {
        _roomService = floorService;
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
    public async Task<ActionResult<List<Room>>> GetRooms(string homeId, string floorId, [FromQuery] SearchOptionsQuery options)
    {
        var rooms = await _roomService.GetFloorRooms((await GetAuthUser()).Id, homeId, floorId, options);
        return rooms;
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> PostRoom(string homeId, string floorId, [FromBody] NameDto dto)
    {
        var room = await _roomService.CreateRoom((await GetAuthUser()).Id, homeId, floorId, dto.Name);
        return CreatedAtAction(nameof(PostRoom), new { id = room.Id }, room);
    }

    [Authorize]
    [HttpPut("{roomId}")]
    public async Task<IActionResult> UpdateRoom(string homeId, string floorId, string roomId, [FromBody] UpdateRoomDto dto)
    {
        await _roomService.UpdateRoom((await GetAuthUser()).Id, homeId, floorId, roomId, dto.Name, dto.IsFavorite);
        return NoContent();
    }

    [Authorize]
    [HttpDelete("{roomId}")]
    public async Task<IActionResult> DeleteRoom(string homeId, string floorId, string roomId)
    {
        await _roomService.DeleteRoom((await GetAuthUser()).Id, homeId, floorId, roomId);
        return NoContent();
    }
}