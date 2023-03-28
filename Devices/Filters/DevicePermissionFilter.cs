using Microsoft.AspNetCore.Mvc.Filters;
using smart_home_server.Home.Services;
using smart_home_server.Exceptions;
using smart_home_server.Auth.Services;

namespace smart_home_server.Devices.Filters;

public class DevicePermissionFilter : IAsyncActionFilter
{
    private readonly IRoomService _roomService;
    private string _role;

    public DevicePermissionFilter(string role, IRoomService roomService)
    {
        _role = role;
        _roomService = roomService;
    }

    public async Task OnActionExecutionAsync(
        ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var roomId = context.ActionArguments["roomId"] as string;
        var userId = context.HttpContext.User.FindFirst(JwtService.USER_ID)?.Value;

        // Bad request when the roomId or userId is not present
        if (roomId == null || userId == null) throw new BadRequestException();

        // Check if the user has the right to access the room device
        var permitted = await _roomService.CheckIfRoomBelongToOwner(userId, roomId);
        if (!permitted) throw new ForbiddenException();
        await next();
    }
}