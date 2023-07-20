using Microsoft.EntityFrameworkCore;
using smart_home_server.Auth.Models;
using smart_home_server.Db;
using smart_home_server.Exceptions;
using smart_home_server.Home.Models;
using smart_home_server.Home.ResourceModels;
using smart_home_server.Utils;

namespace smart_home_server.Home.Services;

public interface IHomeService
{
    Task<SmartHome?> GetHomeById(string homeId, string? userId);
    Task<List<ApplicationUser>> GetHomeInstallers(ApplicationUser user, string homeId);
    Task<List<ApplicationUser>> GetHomeUsers(ApplicationUser user, string homeId);
    Task AddInstallerToHome(ApplicationUser user, string homeId, string password);
    Task AddUserToHome(ApplicationUser user, string homeId, string password);
    Task<SmartHome> CreateHome(ApplicationUser user, CreateHomeDto dto);
    Task DeleteHome(ApplicationUser user, string homeId);
    Task<List<SmartHome>> GetOwnerHome(ApplicationUser user, SearchOptionsQuery options);
    Task<SmartHome> UpdateHome(ApplicationUser user, string homeId, UpdateHomeDto dto);
}

public class HomeService : IHomeService
{
    private readonly AppDbContext _context;
    public HomeService(AppDbContext context)
    {
        _context = context;
    }

    public Task<SmartHome?> GetHomeById(string homeId, string? userId)
    {
        var query = _context.Homes.Where(h => h.Id.ToString() == homeId);
        if (userId != null) query = query.Where(h => h.OwnerId == userId);
        return query
            .Include(h => h.Users)
            .Include(h => h.Installers)
            .FirstOrDefaultAsync();
    }

    public Task<List<SmartHome>> GetOwnerHome(ApplicationUser user, SearchOptionsQuery options)
    {
        var query = _context.Homes.Where(h => h.OwnerId == user.Id);
        if (options.Name != null) query = query.Where(h => h.Name == options.Name);
        var page = options.Page ?? 1;
        var recordPerPages = options.RecordPerPage ?? 10;
        return query.OrderBy(h => h.Id).Skip((page - 1) * recordPerPages).Take(recordPerPages).ToListAsync();
    }

    public async Task<List<ApplicationUser>> GetHomeInstallers(ApplicationUser user, string homeId)
    {
        var home = await _context.Homes
            .Where(h => h.Id.ToString() == homeId && h.OwnerId == user.Id)
            .Include(h => h.Installers)
            .FirstOrDefaultAsync();
        if (home == null) throw new ModelNotFoundException();
        return home.Installers.ToList();
    }

    public async Task<List<ApplicationUser>> GetHomeUsers(ApplicationUser user, string homeId)
    {
        var home = await _context.Homes
            .Where(h => h.Id.ToString() == homeId && h.OwnerId == user.Id)
            .Include(h => h.Users)
            .FirstOrDefaultAsync();
        if (home == null) throw new ModelNotFoundException();
        return home.Users.ToList();
    }

    public async Task<SmartHome> CreateHome(ApplicationUser user, CreateHomeDto dto)
    {
        var home = new SmartHome
        {
            Name = dto.Name,
            Owner = user,
            Description = dto.Description,
            UserPassword = PasswordHasher.CreateMd5(dto.UserPassword),
            InstallerPassword = PasswordHasher.CreateMd5(dto.InstallerPassword),
        };
        _context.Homes.Add(home);
        await _context.SaveChangesAsync();
        return home;
    }

    public async Task AddInstallerToHome(ApplicationUser user, string homeId, string password)
    {
        var home = await GetHomeById(homeId, user.Id);
        if (home == null) throw new ModelNotFoundException();
        if (home.OwnerId == user.Id) throw new BadRequestException("Cannot add installer to home owned by installer");
        if (home.InstallerPassword != PasswordHasher.CreateMd5(password)) throw new BadRequestException("Wrong installer password");
        home.Installers.Add(user);
        await _context.SaveChangesAsync();
    }

    public async Task AddUserToHome(ApplicationUser user, string homeId, string password)
    {
        var home = await GetHomeById(homeId, user.Id);
        if (home == null) throw new ModelNotFoundException();
        if (home.OwnerId == user.Id) throw new BadRequestException("Cannot add user to home owned by user");
        if (home.UserPassword != PasswordHasher.CreateMd5(password)) throw new BadRequestException("Wrong user password");
        home.Users.Add(user);
        await _context.SaveChangesAsync();
    }

    public async Task<SmartHome> UpdateHome(ApplicationUser user, String homeId, UpdateHomeDto dto)
    {
        var home = _context.Homes.Where(h => h.OwnerId == user.Id && h.Id.ToString() == homeId).FirstOrDefault();
        if (home == null) throw new ModelNotFoundException();
        home.Name = dto.Name ?? home.Name;
        home.Description = dto.Description ?? home.Description;
        home.InstallerPassword = dto.InstallerPassword ?? home.InstallerPassword;
        home.UserPassword = dto.UserPassword ?? home.UserPassword;
        await _context.SaveChangesAsync();
        return home;
    }

    public Task DeleteHome(ApplicationUser user, String homeId)
    {
        var home = _context.Homes.Where(h => h.OwnerId == user.Id && h.Id.ToString() == homeId).FirstOrDefault();
        if (home == null) throw new ModelNotFoundException();
        _context.Homes.Remove(home);
        return _context.SaveChangesAsync();
    }
}