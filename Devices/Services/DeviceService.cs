using Microsoft.EntityFrameworkCore;
using smart_home_server.Db;
using smart_home_server.Devices.Models;
using smart_home_server.Home.Services;

namespace smart_home_server.Devices.Services;

public interface IDeviceService
{
    Task<List<Device>> GetRoomDevices(string roomId);
}

public class DeviceService : IDeviceService
{
    private readonly AppDbContext _context;

    public DeviceService(AppDbContext context, IRoomService roomService)
    {
        _context = context;
    }

    public async Task<List<Device>> GetRoomDevices(string roomId)
    {
        var devices = await _context.Devices
            .Where(d => d.RoomId.ToString() == roomId)
            .ToListAsync();
        return devices;
    }
}