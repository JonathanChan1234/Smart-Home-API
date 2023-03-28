
using smart_home_server.Db;
using smart_home_server.Devices.Models;
using smart_home_server.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace smart_home_server.Devices.Services;

public interface IShadeService
{
    Task<Shade> CreateShade(string roomId, string name, bool hasLevel);
    Task DeleteShade(string roomId, string deviceId);
    Task<List<Shade>> GetRoomShades(string roomId);
    Task<Shade> UpdateShadeProperty(string roomId, string deviceId, string name, bool hasLevel);
    Task<Shade> UpdateShadeStatus(string roomId, string deviceId, int level);
}

public class ShadeService : IShadeService
{
    private readonly AppDbContext _context;

    public ShadeService(AppDbContext context)
    {
        _context = context;
    }

    private async Task<Shade?> FindShadeById(string roomId, string deviceId)
    {
        var shade = await _context.Shades
            .Where(shade => shade.Id == new Guid(deviceId) && shade.RoomId.ToString() == roomId)
            .FirstOrDefaultAsync();
        return shade;
    }

    private async Task<Shade?> FindShadeByName(string roomId, string name)
    {
        var shade = await _context.Shades
            .Where(shade => shade.Name == name && shade.RoomId.ToString() == roomId)
            .FirstOrDefaultAsync();
        return shade;
    }

    public async Task<List<Shade>> GetRoomShades(string roomId)
    {
        var shades = await _context.Shades
            .Where(l => l.RoomId == new Guid(roomId))
            .ToListAsync();
        return shades;
    }

    public async Task<Shade> CreateShade(
        string roomId,
        string name,
        bool hasLevel)
    {
        // Check if the light with the same name exists
        var existingShade = await FindShadeByName(roomId, name);
        if (existingShade != null) throw new BadRequestException($"Shade with name {name} is found in the same room");

        var shade = new Shade(name, new Guid(roomId))
        {
            HasLevel = hasLevel
        };
        _context.Shades.Add(shade);
        await _context.SaveChangesAsync();
        return shade;
    }

    public async Task<Shade> UpdateShadeProperty(
        string roomId,
        string deviceId,
        string name,
        bool hasLevel
    )
    {
        var shade = await FindShadeById(roomId, deviceId);
        if (shade == null) throw new ModelNotFoundException($"Shade (id: {deviceId}) already exists");

        // Check if the shade with the same name exists
        var shadeWithSameName = await FindShadeByName(roomId, name);
        if (shadeWithSameName != null) throw new ModelNotFoundException($"Shade (name: {shadeWithSameName}) already exists");

        shade.Name = name;
        shade.HasLevel = hasLevel;
        await _context.SaveChangesAsync();
        return shade;
    }

    public async Task<Shade> UpdateShadeStatus(
        string roomId,
        string deviceId,
        int level
    )
    {
        // Check if the shade with the same name exists
        var shade = await FindShadeById(roomId, deviceId);
        if (shade == null) throw new ModelNotFoundException($"Shade (id: {deviceId}) cannot be found");

        shade.Level = level;
        shade.StatusLastUpdatedAt = DateTime.Now;
        await _context.SaveChangesAsync();
        return shade;
    }

    public async Task DeleteShade(
        string roomId,
        string deviceId
    )
    {
        // Check if the shade with the same name exists
        var shade = await FindShadeById(roomId, deviceId);
        if (shade == null) throw new ModelNotFoundException($"Shade (id: {deviceId}) cannot be found");

        _context.Shades.Remove(shade);
        await _context.SaveChangesAsync();
    }
}