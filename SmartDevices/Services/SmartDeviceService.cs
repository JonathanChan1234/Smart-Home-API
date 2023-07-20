using Microsoft.EntityFrameworkCore;
using smart_home_server.Db;
using smart_home_server.Exceptions;
using smart_home_server.Home.Models;
using smart_home_server.SmartDevices.Models;
using smart_home_server.SmartDevices.ResourceModels;

namespace smart_home_server.SmartDevices.Services;

public interface ISmartDeviceService
{
    Task DeleteDevice(SmartHome home, string deviceId);
    Task<SmartDevice?> FindDeviceById(SmartHome home, string deviceId);
    Task<List<SmartDevice>> FindDevicesInHome(SmartHome home, Room? room = null);
    Task<List<DeviceCount>> FindDeviceCount(SmartHome home, Room? room = null);
    Task UpdateDeviceName(SmartHome home, string deviceId, string name, Room? room = null);
}

public class SmartDeviceService : ISmartDeviceService
{
    private readonly AppDbContext _context;
    public SmartDeviceService(
        AppDbContext context
    )
    {
        _context = context;
    }

    public Task<SmartDevice?> FindDeviceById(
        SmartHome home,
        string deviceId
    )
    {
        return _context.SmartDevices
            .Where(d => d.HomeId == home.Id && d.Id.ToString() == deviceId)
            .FirstOrDefaultAsync();
    }

    public Task<List<SmartDevice>> FindDevicesInHome(
        SmartHome home,
        Room? room = null
    )
    {
        var query = _context.SmartDevices
            .Where(d => d.HomeId == home.Id);
        if (room != null) query = query.Where(d => d.RoomId == room.Id);
        return query.ToListAsync();
    }

    public Task<List<DeviceCount>> FindDeviceCount(
        SmartHome home,
        Room? room = null
    )
    {
        var query = _context.SmartDevices.Where(d => d.HomeId == home.Id);
        if (room != null) query = query.Where(d => d.RoomId == room.Id);
        return query
            .GroupBy(d => d.MainCategory)
            .Select(group => new DeviceCount() { MainCategory = group.Key, Count = group.Count() })
            .ToListAsync();
    }

    public async Task UpdateDeviceName(
        SmartHome home,
        string deviceId,
        string name,
        Room? room = null
    )
    {
        var device = await FindDeviceById(home, deviceId);
        if (device == null) throw new ModelNotFoundException($"device (id: {deviceId}) does not exist");
        device.Name = name;
        if (room != null) device.Room = room;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteDevice(
        SmartHome home,
        string deviceId
    )
    {
        var device = await FindDeviceById(home, deviceId);
        if (device == null) throw new ModelNotFoundException($"device (id: {deviceId}) does not exist");
        _context.SmartDevices.Remove(device);
        await _context.SaveChangesAsync();
    }
}
