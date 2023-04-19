using smart_home_server.Db;
using smart_home_server.Devices.Models;
using smart_home_server.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace smart_home_server.Devices.Services;

public interface ILightService
{
    Task<Light?> FindLightInHome(string homeId, string deviceId);
    Task<Light?> FindLightInRoom(string roomId, string deviceId);
    Task<Light> CreateLight(string roomId, string name, bool dimmable);
    Task DeleteLight(string roomId, string deviceId);
    Task<List<Light>> GetRoomLights(string roomId);
    Task<Light> UpdateLightStatusInHome(string homeId, string deviceId, int level, DateTime time);
    Task<Light> UpdateLightProperty(string roomId, string deviceId, string name, bool dimmable);
}

public class LightService : ILightService
{
    private readonly AppDbContext _context;

    public LightService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Light?> FindLightInHome(string homeId, string deviceId)
    {
        var lightQuery = from l in _context.Lights
                         join r in _context.Rooms on l.RoomId equals r.Id
                         join f in _context.Floors on r.FloorId equals f.Id
                         join h in _context.Homes on f.HomeId equals h.Id
                         where l.Id.ToString() == deviceId
                         where h.Id.ToString() == homeId
                         select l;
        var light = await lightQuery.FirstOrDefaultAsync();
        return light;
    }

    public async Task<Light?> FindLightInRoom(string roomId, string deviceId)
    {
        var light = await _context.Lights
            .Where(light => light.Id.ToString() == deviceId && light.RoomId.ToString() == roomId)
            .FirstOrDefaultAsync();
        return light;
    }

    private async Task<Light?> FindLightByName(string roomId, string name)
    {
        var light = await _context.Lights
            .Where(light => light.Name == name && light.RoomId.ToString() == roomId)
            .FirstOrDefaultAsync();
        return light;
    }

    public async Task<List<Light>> GetRoomLights(string roomId)
    {
        var lights = await _context.Lights
            .Where(l => l.RoomId.ToString() == roomId)
            .ToListAsync();
        return lights;
    }

    public async Task<Light> CreateLight(
        string roomId,
        string name,
        bool dimmable)
    {
        // Check if the light with the same name exists
        var existingLight = await FindLightByName(roomId, name);
        if (existingLight != null) throw new BadRequestException($"Light with name {name} is found in the same room");

        var light = new Light(name, new Guid(roomId))
        {
            Dimmable = dimmable
        };
        _context.Lights.Add(light);
        await _context.SaveChangesAsync();
        return light;
    }

    public async Task<Light> UpdateLightProperty(
        string roomId,
        string deviceId,
        string name,
        bool dimmable
    )
    {
        // Check if the light with the same name exists
        var light = await FindLightInRoom(roomId, deviceId);
        if (light == null) throw new ModelNotFoundException($"Light (id: {deviceId}) cannot be found");

        var lightWithSameName = await FindLightByName(roomId, name);
        if (lightWithSameName != null) throw new ModelNotFoundException($"Light (name: {name}) already exists");

        light.Name = name;
        light.Dimmable = dimmable;
        await _context.SaveChangesAsync();
        return light;
    }

    public async Task<Light> UpdateLightStatusInHome(
        string homeId,
        string deviceId,
        int level,
        DateTime time
    )
    {
        // Check if the light with the same name exists
        var light = await FindLightInHome(homeId, deviceId);
        if (light == null) throw new ModelNotFoundException($"Light {deviceId} cannot be found");

        if (time < light.StatusLastUpdatedAt) throw new BadRequestException($"Status not up to date");

        light.Level = level;
        light.StatusLastUpdatedAt = time;
        await _context.SaveChangesAsync();
        return light;
    }

    public async Task DeleteLight(
        string roomId,
        string deviceId
    )
    {
        // Check if the light with the same name exists
        var light = await FindLightInRoom(roomId, deviceId);
        if (light == null) throw new ModelNotFoundException($"Light {deviceId} cannot be found");

        _context.Lights.Remove(light);
        await _context.SaveChangesAsync();
    }
}