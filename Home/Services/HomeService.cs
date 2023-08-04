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
    Task<SmartHome> AddInstallerToHome(ApplicationUser user, string homeId, string password);
    Task<SmartHome> AddUserToHome(ApplicationUser user, string homeId, string password);
    Task<SmartHome> CreateHome(ApplicationUser user, CreateHomeDto dto);
    Task DeleteHome(SmartHome home);
    Task<List<SmartHome>> GetOwnerHome(ApplicationUser user, SearchOptionsQuery options);
    Task<List<SmartHome>> GetUserHome(ApplicationUser user, SearchOptionsQuery options);
    Task<List<SmartHome>> GetInstallerHome(ApplicationUser user, SearchOptionsQuery options);
    Task RemoveUserFromHome(ApplicationUser user, string homeId);
    Task RemoveInstallerFromHome(ApplicationUser user, string homeId);
    Task<SmartHome> UpdateHome(SmartHome home, UpdateHomeDto dto);
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

    public Task<List<SmartHome>> GetUserHome(ApplicationUser user, SearchOptionsQuery options)
    {
        var query = _context.Homes.Where(h => h.Users.Contains(user));
        if (options.Name != null) query = query.Where(h => h.Name == options.Name);
        var page = options.Page ?? 1;
        var recordPerPages = options.RecordPerPage ?? 10;
        return query.OrderBy(h => h.Id).Skip((page - 1) * recordPerPages).Take(recordPerPages).ToListAsync();
    }

    public Task<List<SmartHome>> GetInstallerHome(ApplicationUser user, SearchOptionsQuery options)
    {
        var query = _context.Homes.Where(h => h.Installers.Contains(user));
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

    public async Task<SmartHome> AddInstallerToHome(ApplicationUser user, string homeId, string password)
    {
        var home = await GetHomeById(homeId, null);
        if (home == null) throw new ModelNotFoundException();
        if (home.OwnerId == user.Id) throw new BadRequestException("Cannot add installer to home owned by installer");

        var existingInstaller = home.Installers.FirstOrDefault(u => u.Id == user.Id);
        if (existingInstaller != null) throw new BadRequestException("User already added to this home");

        if (home.InstallerPassword != PasswordHasher.CreateMd5(password)) throw new BadRequestException("Wrong installer password");
        home.Installers.Add(user);
        await _context.SaveChangesAsync();
        return home;
    }

    public async Task<SmartHome> AddUserToHome(ApplicationUser user, string homeId, string password)
    {
        var home = await GetHomeById(homeId, null);
        if (home == null) throw new ModelNotFoundException($"home {homeId} does not exist");
        if (home.OwnerId == user.Id) throw new BadRequestException("Cannot add user to home owned by user");

        var existingUser = home.Users.FirstOrDefault(u => u.Id == user.Id);
        if (existingUser != null) throw new BadRequestException("User already added to this home");

        if (home.UserPassword != PasswordHasher.CreateMd5(password)) throw new BadRequestException("Wrong user password");
        home.Users.Add(user);
        await _context.SaveChangesAsync();
        return home;
    }

    public async Task<SmartHome> UpdateHome(SmartHome home, UpdateHomeDto dto)
    {
        home.Name = dto.Name ?? home.Name;
        home.Description = dto.Description ?? home.Description;
        home.InstallerPassword = dto.InstallerPassword ?? home.InstallerPassword;
        home.UserPassword = dto.UserPassword ?? home.UserPassword;
        await _context.SaveChangesAsync();
        return home;
    }

    public async Task RemoveUserFromHome(ApplicationUser user, string homeId)
    {
        var home = await GetHomeById(homeId, null);
        if (home == null) throw new ModelNotFoundException();

        var existingUser = home.Users.FirstOrDefault(u => u.Id == user.Id);
        if (existingUser == null) throw new BadRequestException("User does not belong to this home");

        home.Users.Remove(user);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveInstallerFromHome(ApplicationUser user, string homeId)
    {
        var home = await GetHomeById(homeId, null);
        if (home == null) throw new ModelNotFoundException();

        var existingInstaller = home.Installers.FirstOrDefault(u => u.Id == user.Id);
        if (existingInstaller == null) throw new BadRequestException("Installer does not belong to this home");

        home.Installers.Remove(user);
        await _context.SaveChangesAsync();
    }

    public Task DeleteHome(SmartHome home)
    {
        _context.Homes.Remove(home);
        return _context.SaveChangesAsync();
    }
}