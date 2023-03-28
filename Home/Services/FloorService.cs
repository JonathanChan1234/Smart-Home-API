using Microsoft.EntityFrameworkCore;
using smart_home_server.Db;
using smart_home_server.Exceptions;
using smart_home_server.Home.Exceptions;
using smart_home_server.Home.Models;
using smart_home_server.Home.ResourceModels;
using smart_home_server.Home.Services;

public interface IFloorService
{
    Task<Floor> CreateFloor(string userId, string homeId, string name);
    Task DeleteFloor(string userId, string homeId, string floorId);
    Task<Floor?> GetFloorById(string homeId, string floorId);
    Task<List<Floor>> GetHomeFloors(string userId, string homeId, SearchOptionsQuery options);
    Task UpdateFloor(string userId, string homeId, string floorId, string name);
}

public class FloorService : IFloorService
{
    private readonly AppDbContext _context;
    private readonly IHomeService _homeService;
    public FloorService(AppDbContext context, IHomeService homeService)
    {
        _context = context;
        _homeService = homeService;
    }

    public async Task<Floor?> GetFloorById(string homeId, string floorId)
    {
        var floor = await _context.Floors
            .Where(f => f.HomeId.ToString() == homeId && f.Id.ToString() == floorId)
            .FirstOrDefaultAsync();
        return floor;
    }

    public async Task<List<Floor>> GetHomeFloors(string userId, string homeId, SearchOptionsQuery options)
    {
        var home = await _homeService.GetHomeById(homeId, userId);
        if (home == null) throw new HomeNotFoundException();

        var page = options.Page ?? 1;
        var recordPerPage = options.RecordPerPage ?? 10;
        var query = _context.Floors.Where(f => f.HomeId.ToString() == homeId);
        if (options.Name != null) query = query.Where(f => f.Name == options.Name);
        var floors = await query.OrderBy(f => f.Id).Skip((page - 1) * recordPerPage).Take(recordPerPage).Include(f => f.Rooms).ToListAsync();
        return floors;
    }

    public async Task<Floor> CreateFloor(string userId, string homeId, string name)
    {
        var home = await _homeService.GetHomeById(homeId, userId);
        if (home == null) throw new HomeNotFoundException();

        var searchFloor = await _context.Floors.Where(f => f.HomeId.ToString() == homeId && f.Name == name).FirstOrDefaultAsync();
        if (searchFloor != null) throw new BadRequestException("Floor name already exists in the same home");
        var floor = new Floor() { Name = name, Home = home };
        _context.Floors.Add(floor);
        await _context.SaveChangesAsync();
        return floor;
    }

    public async Task UpdateFloor(string userId, string homeId, string floorId, string name)
    {
        var home = await _homeService.GetHomeById(homeId, userId);
        if (home == null) throw new HomeNotFoundException();

        var floor = await GetFloorById(homeId, floorId);
        if (floor == null) throw new BadRequestException("Floor Id does not exist");
        var searchFloor = await _context.Floors
            .Where(f => f.Name == name)
            .FirstOrDefaultAsync();
        if (searchFloor != null) throw new BadRequestException("Floor name already exists in the same home");
        floor.Name = name;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteFloor(string userId, string homeId, string floorId)
    {
        var home = await _homeService.GetHomeById(homeId, userId);
        if (home == null) throw new HomeNotFoundException();

        var floor = await GetFloorById(homeId, floorId);
        if (floor == null) throw new BadRequestException("Floor Id does not exist");
        _context.Floors.Remove(floor);
        await _context.SaveChangesAsync();
    }
}