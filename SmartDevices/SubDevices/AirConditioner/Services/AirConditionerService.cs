using Microsoft.AspNetCore.Http.HttpResults;
using smart_home_server.Db;
using smart_home_server.Exceptions;
using smart_home_server.Home.Models;
using smart_home_server.SmartDevices.Models;
using smart_home_server.SmartDevices.Services;
using smart_home_server.SmartDevices.SubDevices.AirConditioner.Models;
using smart_home_server.Utils;

namespace smart_home_server.SmartDevices.SubDevices.AirConditioner.Service;

public interface IAirConditionerService
{
    Task<SmartDevice> FindAirConditionerById(SmartHome home, string deviceId);
    Task<List<SmartDevice>> FindAirConditionerInHome(SmartHome home, Room? room = null);
    Task<SmartDevice> CreateAirConditioner(SmartHome home, Room room, string name, AirConditionerCapabilities capabilities);
    Task UpdateAirConditionerStatus(SmartHome home, string deviceId, DateTime lastUpdatedAt, AirConditionerProperties? properties, bool? onlineStatus);
}

public class AirConditionerService : IAirConditionerService
{
    private readonly AppDbContext _context;
    private readonly ISmartDeviceService _smartDeviceService;

    public AirConditionerService(
        AppDbContext context,
        ISmartDeviceService smartDeviceService)
    {
        _smartDeviceService = smartDeviceService;
        _context = context;
    }

    public async Task<SmartDevice> FindAirConditionerById(
        SmartHome home,
        string deviceId
    )
    {
        var device = await _smartDeviceService
            .FindDeviceById(home, deviceId, MainCategory.AirConditioner);
        return device ?? throw new ModelNotFoundException($"Device (id: {deviceId}) does not exist");
    }

    public async Task<List<SmartDevice>> FindAirConditionerInHome(
        SmartHome home,
        Room? room = null
    )
    {
        var devices = await _smartDeviceService.FindDevicesInHome(home, room, MainCategory.AirConditioner);
        return devices;
    }

    public Task<SmartDevice> CreateAirConditioner(
        SmartHome home,
        Room room,
        string name,
        AirConditionerCapabilities capabilities
    )
    {
        return _smartDeviceService.CreateDevice
            <AirConditionerCapabilities, AirConditionerProperties>(
            home,
            room,
            name,
            MainCategory.AirConditioner,
            SubCategory.AirConditioner,
            capabilities
        );
    }

    public async Task UpdateAirConditionerStatus(
        SmartHome home,
        string deviceId,
        DateTime lastUpdatedAt,
        AirConditionerProperties? properties,
        bool? onlineStatus
    )
    {
        var ac = await FindAirConditionerById(home, deviceId);
        if (ac.StatusLastUpdatedAt > lastUpdatedAt) throw new BadRequestException("Update AC status failed. Reason: The status is not up to date");
        ac.StatusLastUpdatedAt = lastUpdatedAt;
        if (properties != null)
            ac.Properties = ac.Properties.UpdateDict(properties);
        if (onlineStatus != null)
            ac.OnlineStatus = (bool)onlineStatus;
        await _context.SaveChangesAsync();
    }
}