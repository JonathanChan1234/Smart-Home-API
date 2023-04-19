using Microsoft.EntityFrameworkCore;
using smart_home_server.Db;
using smart_home_server.Devices.Models;
using smart_home_server.Devices.ResourceModels;
using smart_home_server.Home.Services;

namespace smart_home_server.Devices.Services;
enum DeviceType
{
    light,
    shade,
}

public interface IDeviceService
{
    Task<List<DeviceDto>> GetRoomDevices(string roomId);
}

public class DeviceService : IDeviceService
{
    private readonly AppDbContext _context;

    public DeviceService(AppDbContext context, IRoomService roomService)
    {
        _context = context;
    }

    public async Task<List<DeviceDto>> GetRoomDevices(string roomId)
    {
        var lightCount = await _context.Lights
            .Where(l => l.RoomId.ToString() == roomId)
            .CountAsync();
        var shadeCount = await _context.Shades
            .Where(s => s.RoomId.ToString() == roomId)
            .CountAsync();
        var lightDeviceDto = new DeviceDto { DeviceType = DeviceType.light.ToString(), NumberOfDevices = lightCount };
        var shadeDeviceDto = new DeviceDto { DeviceType = DeviceType.shade.ToString(), NumberOfDevices = shadeCount };
        return new List<DeviceDto>{
            lightDeviceDto, shadeDeviceDto
        };
    }
}