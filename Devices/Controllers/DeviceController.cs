
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using smart_home_server.Devices.Filters;
using smart_home_server.Devices.ResourceModels;
using smart_home_server.Devices.Services;

[ApiController]
[Route("api/v1/room/{roomId}/[controller]")]
public class DeviceController : ControllerBase
{
    private readonly IDeviceService _deviceService;
    public DeviceController(IDeviceService deviceService)
    {
        _deviceService = deviceService;
    }

    [TypeFilter(typeof(DevicePermissionFilter), Arguments = new object[] { "installer" })]
    [Authorize]
    [HttpGet]
    public async Task<ActionResult<List<DeviceDto>>> GetLight(string roomId)
    {
        var devices = await _deviceService.GetRoomDevices(roomId);
        return devices;
    }
}