using Microsoft.EntityFrameworkCore;
using smart_home_server.Db;
using smart_home_server.Exceptions;
using smart_home_server.Home.Models;
using smart_home_server.Home.ResourceModels;

namespace smart_home_server.Home.Services;

public interface IRoomService
{
    Task<Room> CreateRoom(SmartHome home, string floorId, string name);
    Task DeleteRoom(SmartHome home, string floorId, string roomId);
    Task<List<Room>> GetFloorRooms(SmartHome home, string floorId, SearchOptionsQuery options);
    Task<Room?> GetRoomById(string floorId, string roomId);
    Task<Room?> GetRoomInHome(SmartHome home, string roomId);
    Task UpdateRoom(SmartHome home, string floorId, string roomId, string? name, bool? isFavorite);
}

public class RoomService : IRoomService
{
    private readonly AppDbContext _context;
    private readonly IFloorService _floorService;
    public RoomService(AppDbContext context, IFloorService floorService)
    {
        _context = context;
        _floorService = floorService;
    }

    public async Task<Room?> GetRoomById(string floorId, string roomId)
    {
        var room = await _context.Rooms
            .Where(r => r.Id.ToString() == roomId && r.FloorId.ToString() == floorId)
            .FirstOrDefaultAsync();
        return room;
    }

    public async Task<Room?> GetRoomInHome(SmartHome home, string roomId)
    {
        var roomQuery = from r in _context.Rooms
                        join f in _context.Floors on r.FloorId equals f.Id
                        join h in _context.Homes on f.HomeId equals h.Id
                        where (r.Id.ToString() == roomId && h.Id == home.Id)
                        select r;
        return (await roomQuery.FirstOrDefaultAsync());
    }

    public async Task<List<Room>> GetFloorRooms(SmartHome home, string floorId, SearchOptionsQuery options)
    {
        var floor = await _floorService.GetFloorById(home, floorId);
        if (floor == null) throw new ModelNotFoundException($"floor id {floorId} does not exist");

        var page = options.Page ?? 1;
        var recordPerPage = options.RecordPerPage ?? 10;
        var query = _context.Rooms.Where(r => r.FloorId == floor.Id);
        if (options.Name != null) query = query.Where(f => f.Name == options.Name);
        var rooms = await query.OrderBy(f => f.Id).Skip((page - 1) * recordPerPage).Take(recordPerPage).ToListAsync();
        return rooms;
    }

    public async Task<Room> CreateRoom(SmartHome home, string floorId, string name)
    {
        var floor = await _floorService.GetFloorById(home, floorId);
        if (floor == null) throw new ModelNotFoundException($"floor id {floorId} does not exist");

        var searchRoom = await _context.Rooms.Where(r => r.FloorId == floor.Id && r.Name == name).FirstOrDefaultAsync();
        if (searchRoom != null) throw new BadRequestException("Room name already exists in the same home");
        var room = new Room() { Name = name, Floor = floor };
        _context.Rooms.Add(room);
        await _context.SaveChangesAsync();
        return room;
    }

    public async Task UpdateRoom(SmartHome home, string floorId, string roomId, string? name, bool? isFavorite)
    {
        var floor = await _floorService.GetFloorById(home, floorId);
        if (floor == null) throw new ModelNotFoundException($"floor id {floorId} does not exist");

        var searchRoom = await _context.Rooms.Where(r => r.FloorId == floor.Id && r.Name == name).FirstOrDefaultAsync();
        if (searchRoom != null && searchRoom.Id.ToString() != roomId) throw new BadRequestException("Room name already exists in the same home");

        var room = await GetRoomById(floorId, roomId);
        if (room == null) throw new ModelNotFoundException();
        if (name != null) room.Name = name;
        if (isFavorite != null) room.IsFavorite = (bool)isFavorite;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteRoom(SmartHome home, string floorId, string roomId)
    {
        var floor = await _floorService.GetFloorById(home, floorId);
        if (floor == null) throw new ModelNotFoundException($"floor id {floorId} does not exist");

        var room = await GetRoomById(floor.Id.ToString(), roomId);
        if (room == null) throw new ModelNotFoundException();

        _context.Rooms.Remove(room);
        await _context.SaveChangesAsync();
    }
}