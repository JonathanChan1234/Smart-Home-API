using Microsoft.EntityFrameworkCore;
using smart_home_server.Db;
using smart_home_server.Exceptions;
using smart_home_server.Home.Models;
using smart_home_server.SmartDevices.Models;
using smart_home_server.SmartDevices.SubDevices.Lights.Models;
using smart_home_server.Utils;

namespace smart_home_server.SmartDevices.SubDevices.Lights.Service;

public interface ISmartLightService
{
    Task<SmartDevice> FindLightById(SmartHome home, string deviceId);
    Task<List<SmartDevice>> FindLightsInHome(SmartHome home, Room? room = null);
    Task<SmartDevice> CreateLightDevice(SmartHome home, Room room, string name, SubCategory subCategory, LightCapabilities capabilities);
    Task UpdateLightStatus(SmartHome home, string deviceId, DateTime lastUpdatedAt, LightProperties? dto = null, bool? OnlinStatus = null);
    Task UpdateLightCapabilities(SmartHome home, string deviceId, LightCapabilities dto);
}

public class SmartLightService : ISmartLightService
{
    private readonly AppDbContext _context;

    public SmartLightService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<SmartDevice> FindLightById(
        SmartHome home,
        string deviceId
    )
    {
        var light = await _context.SmartDevices
            .Where(
                d => d.MainCategory == MainCategory.Light &&
                d.Id.ToString() == deviceId &&
                d.HomeId == home.Id)
            .FirstOrDefaultAsync();
        if (light == null)
            throw new ModelNotFoundException($"light (id: {deviceId}) does not exist");
        return light;
    }

    public Task<List<SmartDevice>> FindLightsInHome(
        SmartHome home,
        Room? room = null
    )
    {
        var query = _context.SmartDevices.Where(
            d => d.MainCategory == MainCategory.Light &&
            d.HomeId == home.Id
        );
        if (room != null) query = query.Where(d => d.RoomId == room.Id);
        return query.ToListAsync();
    }

    public async Task<SmartDevice> CreateLightDevice(
        SmartHome home,
        Room room,
        string name,
        SubCategory subCategory,
        LightCapabilities capabilities
    )
    {
        var light = new SmartDevice
        {
            Home = home,
            Room = room,
            MainCategory = MainCategory.Light,
            SubCategory = subCategory,
            OnlineStatus = true,
            Name = name,
            Properties = new LightProperties().ToDict<LightProperties>(),
            Capabilities = capabilities.ToDict<LightCapabilities>()
        };
        _context.SmartDevices.Add(light);
        await _context.SaveChangesAsync();
        return light;
    }

    public async Task UpdateLightStatus(
        SmartHome home,
        string deviceId,
        DateTime lastUpdatedAt,
        LightProperties? properties = null,
        bool? onlineStatus = null
    )
    {
        var light = await FindLightById(home, deviceId);
        light.StatusLastUpdatedAt = lastUpdatedAt;
        if (properties != null) light.Properties = light.Properties.UpdateDict<LightProperties>(properties);
        if (onlineStatus != null) light.OnlineStatus = (bool)onlineStatus;
        await _context.SaveChangesAsync();
    }

    public async Task UpdateLightCapabilities(
        SmartHome home,
        string deviceId,
        LightCapabilities capabilities
    )
    {
        var light = await FindLightById(home, deviceId);
        light.Capabilities = capabilities.ToDict<LightCapabilities>();
        await _context.SaveChangesAsync();
    }
}