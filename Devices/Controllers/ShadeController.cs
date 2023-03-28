
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using smart_home_server.Devices.Models;
using smart_home_server.Devices.ResourceModels;
using smart_home_server.Devices.Services;
using smart_home_server.Devices.Filters;

[ApiController]
[Route("api/v1/room/{roomId}/device/[controller]")]
public class ShadeController : ControllerBase
{
    private readonly IShadeService _shadeService;
    public ShadeController(IShadeService shadeService)
    {
        _shadeService = shadeService;
    }

    [TypeFilter(typeof(DevicePermissionFilter), Arguments = new object[] { "installer" })]
    [Authorize]
    [HttpGet]
    public async Task<ActionResult<List<Shade>>> GetShades(string roomId)
    {
        var shades = await _shadeService.GetRoomShades(roomId);
        return shades;
    }

    [TypeFilter(typeof(DevicePermissionFilter), Arguments = new object[] { "installer" })]
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> PostShade(string roomId, [FromBody] CreateShadeDto dto)
    {
        var shade = await _shadeService.CreateShade(
            roomId,
            dto.Name,
            dto.HasLevel);
        return CreatedAtAction(nameof(PostShade), new { id = shade.Id }, shade);
    }

    [TypeFilter(typeof(DevicePermissionFilter), Arguments = new object[] { "installer" })]
    [Authorize]
    [HttpPut("{deviceId}")]
    public async Task<IActionResult> PutShade(string roomId, string deviceId, [FromBody] UpdateShadeDto dto)
    {
        var shade = await _shadeService.UpdateShadeProperty(
            roomId,
            deviceId,
            dto.Name,
            dto.HasLevel);
        return NoContent();
    }

    [TypeFilter(typeof(DevicePermissionFilter), Arguments = new object[] { "installer" })]
    [Authorize]
    [HttpDelete("{deviceId}")]
    public async Task<IActionResult> DeleteLight(string roomId, string deviceId)
    {
        await _shadeService.DeleteShade(
            roomId,
            deviceId);
        return NoContent();
    }
}