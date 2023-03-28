using Microsoft.EntityFrameworkCore;
using smart_home_server.Db;
using smart_home_server.Exceptions;
using smart_home_server.Home.Models;
using smart_home_server.Home.ResourceModels;

namespace smart_home_server.Home.Services;

public interface IRoomService
{
    Task<Room> CreateRoom(string userId, string homeId, string floorId, string name);
    Task DeleteRoom(string userId, string homeId, string floorId, string roomId);
    Task<List<Room>> GetFloorRooms(string userId, string homeId, string floorId, SearchOptionsQuery options);
    Task<Room?> GetRoomById(string floorId, string roomId);
    Task<bool> CheckIfRoomBelongToOwner(string userId, string roomId);
    Task UpdateRoom(string userId, string homeId, string floorId, string roomId, string? name, bool? isFavorite);
}

class RoomService : IRoomService
{
    private readonly AppDbContext _context;
    private readonly IFloorService _floorService;
    private readonly IHomeService _homeService;
    public RoomService(AppDbContext context, IFloorService floorService, IHomeService homeService)
    {
        _context = context;
        _floorService = floorService;
        _homeService = homeService;
    }

    /// <summary>
    /// Method <c>CheckFloorPermission</c> check if the current user owns the floor and return the floor object if permitted
    /// Throw ModelNotFoundException if the floor was not found or not owned by users
    /// </summary>
    private async Task<Floor> CheckFloorPermission(string userId, string homeId, string floorId)
    {
        var home = await _homeService.GetHomeById(homeId, userId);
        if (home == null) throw new ModelNotFoundException();

        var floor = await _floorService.GetFloorById(homeId, floorId);
        if (floor == null) throw new ModelNotFoundException();
        return floor;
    }

    public async Task<bool> CheckIfRoomBelongToOwner(string userId, string roomId)
    {
        var roomQuery = from r in _context.Rooms
                        join f in _context.Floors on r.FloorId equals f.Id
                        join h in _context.Homes on f.HomeId equals h.Id
                        join u in _context.Users on h.OwnerId equals u.Id
                        where u.Id == userId
                        where r.Id == new Guid(roomId)
                        select r;
        var room = await roomQuery.FirstOrDefaultAsync();
        return room != null;
    }

    public async Task<bool> CheckIfRoomBelongToInstaller(string userId, string roomId)
    {
        var roomQuery = from r in _context.Rooms
                        join f in _context.Floors on r.FloorId equals f.Id
                        join h in _context.Homes on f.HomeId equals h.Id
                        join u in _context.Users on h.OwnerId equals u.Id
                        where (from installer in h.Installers
                               where u.Id == userId
                               select installer
                        ).Any() == true
                        where r.Id == new Guid(roomId)
                        select r;
        var room = await roomQuery.FirstOrDefaultAsync();
        return room != null;
    }

    public async Task<Room?> GetRoomById(string floorId, string roomId)
    {
        var room = await _context.Rooms
            .Where(r => r.Id.ToString() == roomId && r.FloorId.ToString() == floorId)
            .FirstOrDefaultAsync();
        return room;
    }

    public async Task<List<Room>> GetFloorRooms(string userId, string homeId, string floorId, SearchOptionsQuery options)
    {
        var floor = await CheckFloorPermission(userId, homeId, floorId);

        var page = options.Page ?? 1;
        var recordPerPage = options.RecordPerPage ?? 10;
        var query = _context.Rooms.Where(r => r.FloorId == floor.Id);
        if (options.Name != null) query = query.Where(f => f.Name == options.Name);
        var rooms = await query.OrderBy(f => f.Id).Skip((page - 1) * recordPerPage).Take(recordPerPage).ToListAsync();
        return rooms;
    }

    public async Task<Room> CreateRoom(string userId, string homeId, string floorId, string name)
    {
        var floor = await CheckFloorPermission(userId, homeId, floorId);

        var searchRoom = await _context.Rooms.Where(r => r.FloorId.ToString() == floorId && r.Name == name).FirstOrDefaultAsync();
        if (searchRoom != null) throw new BadRequestException("Room name already exists in the same home");
        var room = new Room() { Name = name, Floor = floor };
        _context.Rooms.Add(room);
        await _context.SaveChangesAsync();
        return room;
    }

    public async Task UpdateRoom(string userId, string homeId, string floorId, string roomId, string? name, bool? isFavorite)
    {
        var floor = await CheckFloorPermission(userId, homeId, floorId);

        var searchRoom = await _context.Rooms.Where(r => r.FloorId.ToString() == floorId && r.Name == name).FirstOrDefaultAsync();
        if (searchRoom != null && searchRoom.Id.ToString() != roomId) throw new BadRequestException("Room name already exists in the same home");
        var room = await GetRoomById(floorId, roomId);
        if (room == null) throw new ModelNotFoundException();
        if (name != null) room.Name = name;
        if (isFavorite != null) room.IsFavorite = (bool)isFavorite;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteRoom(string userId, string homeId, string floorId, string roomId)
    {
        var floor = await CheckFloorPermission(userId, homeId, floorId);

        var room = await GetRoomById(floor.Id.ToString(), roomId);
        if (room == null) throw new ModelNotFoundException();
        _context.Rooms.Remove(room);
        await _context.SaveChangesAsync();
    }
}