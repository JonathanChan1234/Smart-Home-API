using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using smart_home_server.Devices.Models;
using smart_home_server.Devices.ResourceModels;
using smart_home_server.Devices.Services;
using smart_home_server.Devices.Filters;

[ApiController]
[Route("api/v1/room/{roomId}/device/[controller]")]
public class LightController : ControllerBase
{
    private readonly ILightService _lightService;
    public LightController(ILightService lightService)
    {
        _lightService = lightService;
    }

    [TypeFilter(typeof(DevicePermissionFilter), Arguments = new object[] { "installer" })]
    [Authorize]
    [HttpGet]
    public async Task<ActionResult<List<Light>>> GetLight(string roomId)
    {
        var lights = await _lightService.GetRoomLights(roomId);
        return lights;
    }

    [TypeFilter(typeof(DevicePermissionFilter), Arguments = new object[] { "installer" })]
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> PostLight(string roomId, [FromBody] CreateLightDto dto)
    {
        var light = await _lightService.CreateLight(
            roomId,
            dto.Name,
            dto.Dimmable);
        return CreatedAtAction(nameof(PostLight), new { id = light.Id }, light);
    }

    [TypeFilter(typeof(DevicePermissionFilter), Arguments = new object[] { "installer" })]
    [Authorize]
    [HttpPut("{deviceId}")]
    public async Task<IActionResult> PutLight(string roomId, string deviceId, [FromBody] UpdateLightDto dto)
    {
        var light = await _lightService.UpdateLightProperty(
            roomId,
            deviceId,
            dto.Name,
            dto.Dimmable);
        return NoContent();
    }

    [TypeFilter(typeof(DevicePermissionFilter), Arguments = new object[] { "installer" })]
    [Authorize]
    [HttpDelete("{deviceId}")]
    public async Task<IActionResult> DeleteLight(string roomId, string deviceId)
    {
        await _lightService.DeleteLight(
            roomId,
            deviceId);
        return NoContent();
    }
}