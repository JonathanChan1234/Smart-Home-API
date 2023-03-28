using smart_home_server.Db;
using smart_home_server.Devices.Models;
using smart_home_server.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace smart_home_server.Devices.Services;

public interface ILightService
{
    Task<Light> CreateLight(string roomId, string name, bool dimmable);
    Task DeleteLight(string roomId, string deviceId);
    Task<List<Light>> GetRoomLights(string roomId);
    Task<Light> UpdateLightStatus(string roomId, string deviceId, int level);
    Task<Light> UpdateLightProperty(string roomId, string deviceId, string name, bool dimmable);
}

public class LightService : ILightService
{
    private readonly AppDbContext _context;

    public LightService(AppDbContext context)
    {
        _context = context;
    }

    private async Task<Light?> FindLightById(string roomId, string deviceId)
    {
        var light = await _context.Lights
            .Where(light => light.Id == new Guid(deviceId) && light.RoomId.ToString() == roomId)
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
            .Where(l => l.RoomId == new Guid(roomId))
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
        var light = await FindLightById(roomId, deviceId);
        if (light == null) throw new ModelNotFoundException($"Light (id: {deviceId}) cannot be found");

        var lightWithSameName = await FindLightByName(roomId, name);
        if (lightWithSameName != null) throw new ModelNotFoundException($"Light (name: {name}) already exists");

        light.Name = name;
        light.Dimmable = dimmable;
        await _context.SaveChangesAsync();
        return light;
    }

    public async Task<Light> UpdateLightStatus(
        string roomId,
        string deviceId,
        int level
    )
    {
        // Check if the light with the same name exists
        var light = await FindLightById(roomId, deviceId);
        if (light == null) throw new ModelNotFoundException($"Light {deviceId} cannot be found");

        light.Level = level;
        light.StatusLastUpdatedAt = DateTime.Now;
        await _context.SaveChangesAsync();
        return light;
    }

    public async Task DeleteLight(
        string roomId,
        string deviceId
    )
    {
        // Check if the light with the same name exists
        var light = await FindLightById(roomId, deviceId);
        if (light == null) throw new ModelNotFoundException($"Light {deviceId} cannot be found");

        _context.Lights.Remove(light);
        await _context.SaveChangesAsync();
    }
}