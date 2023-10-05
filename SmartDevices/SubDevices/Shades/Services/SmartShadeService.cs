using Microsoft.EntityFrameworkCore;
using smart_home_server.Db;
using smart_home_server.Exceptions;
using smart_home_server.Home.Models;
using smart_home_server.SmartDevices.Models;
using smart_home_server.SmartDevices.SubDevices.Shades.Models;
using smart_home_server.Utils;

namespace smart_home_server.SmartDevices.SubDevices.Shades.Services;

public interface ISmartShadeService
{
    Task<SmartDevice> FindShadeById(SmartHome home, string deviceId);
    Task<List<SmartDevice>> FindShadeInHome(SmartHome home, Room? room = null);
    Task<SmartDevice> CreateShadeDevice(SmartHome home, Room room, string name, SubCategory subCategory, ShadeCapabilities capabilities);
    Task UpdateShadeStatus(SmartHome home, string deviceId, DateTime lastUpdatedAt, ShadeProperties? properties = null, bool? onlineStatus = null);
    Task UpdateShadeCapabilities(SmartHome home, string deviceId, ShadeCapabilities capabilities);
}

public class SmartShadeService : ISmartShadeService
{
    private readonly AppDbContext _context;

    public SmartShadeService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<SmartDevice> FindShadeById(
        SmartHome home,
        string deviceId
    )
    {
        var shade = await _context.SmartDevices
            .Where(
                d => d.MainCategory == MainCategory.Shade &&
                d.HomeId == home.Id &&
                d.Id.ToString() == deviceId
            )
            .FirstOrDefaultAsync();
        if (shade == null) throw new ModelNotFoundException($"shade (device id: {deviceId}) does not exist");
        return shade;
    }

    public Task<List<SmartDevice>> FindShadeInHome(
        SmartHome home,
        Room? room = null
    )
    {
        var query = _context.SmartDevices.Where(
            d => d.MainCategory == MainCategory.Shade &&
            d.HomeId == home.Id
        );
        if (room != null) query = query.Where(d => d.RoomId == room.Id);
        return query.ToListAsync();
    }

    public async Task<SmartDevice> CreateShadeDevice(
        SmartHome home,
        Room room,
        string name,
        SubCategory subCategory,
        ShadeCapabilities capabilities
    )
    {
        var shade = new SmartDevice
        {
            Home = home,
            Room = room,
            MainCategory = MainCategory.Shade,
            SubCategory = subCategory,
            OnlineStatus = true,
            Name = name,
            Properties = new ShadeProperties() { Level = 0 }.ToDict<ShadeProperties>(),
            Capabilities = capabilities.ToDict<ShadeCapabilities>()
        };
        _context.SmartDevices.Add(shade);
        await _context.SaveChangesAsync();
        return shade;
    }

    public async Task UpdateShadeStatus(
        SmartHome home,
        string deviceId,
        DateTime lastUpdatedAt,
        ShadeProperties? properties = null,
        bool? onlineStatus = null)
    {
        var shade = await FindShadeById(home, deviceId);
        if (shade.StatusLastUpdatedAt > lastUpdatedAt) throw new BadRequestException("Update AC status failed. Reason: The status is not up to date");
        shade.StatusLastUpdatedAt = lastUpdatedAt;
        if (properties != null) shade.Properties = shade.Properties.UpdateDict<ShadeProperties>(properties);
        if (onlineStatus != null) shade.OnlineStatus = (bool)onlineStatus;
        await _context.SaveChangesAsync();
    }

    public async Task UpdateShadeCapabilities(SmartHome home, string deviceId, ShadeCapabilities capabilities)
    {
        var shade = await FindShadeById(home, deviceId);
        shade.Capabilities = capabilities.ToDict<ShadeCapabilities>();
        await _context.SaveChangesAsync();
    }
}