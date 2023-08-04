using Microsoft.EntityFrameworkCore;
using smart_home_server.Auth.Models;
using smart_home_server.Db;
using smart_home_server.Exceptions;
using smart_home_server.Home.Models;
using smart_home_server.Mqtt.Client.Services;
using smart_home_server.Processors.Models;
using smart_home_server.Utils;

namespace smart_home_server.Processors.Service;

public interface IProcessorService
{
    Task<Processor> CreateNewProcessor(ApplicationUser user, SmartHome home);
    Task<Processor?> GetProcessorByClientId(int clientId);
    Task<Processor?> GetProcessorByHomeId(string homeId);
    Task UpdateProcessorOnlineStatus(string homeId, bool onlineStatus);
    Task DeleteProcessorAtHome(string homeId);
}

public class ProcessorService : IProcessorService
{
    private readonly AppDbContext _context;
    private readonly IMqttClientService _mqttClientService;

    public ProcessorService(
        AppDbContext context,
        IMqttClientService mqttClientService)
    {
        _context = context;
        _mqttClientService = mqttClientService;
    }

    public Task<Processor?> GetProcessorByHomeId(string homeId)
    {
        return _context.Processors
            .FirstOrDefaultAsync(
                p => p.Home.Id.ToString() == homeId);
    }

    public Task<Processor?> GetProcessorByClientId(int clientId)
    {
        return _context.Processors.FirstOrDefaultAsync(p => p.MqttClientId == clientId);
    }

    public async Task<Processor> CreateNewProcessor(ApplicationUser user, SmartHome home)
    {
        if (home.ProcessorId != null) throw new BadRequestException("You can only have one processor for one home");
        var mqttClient = await _mqttClientService.RegisterMqttClient(user, home);

        var processor = new Processor()
        {
            Home = home,
            MqttClient = mqttClient,
            OnlineStatus = false,
            AddedAt = DateTime.UtcNow,
            Password = StringUtils.RandomString(10)
        };
        _context.Processors.Add(processor);
        await _context.SaveChangesAsync();

        return processor;
    }

    public async Task UpdateProcessorOnlineStatus(string homeId, bool onlineStatus)
    {
        var processor = await GetProcessorByHomeId(homeId);
        if (processor == null) throw new ModelNotFoundException($"Processor does not exist in home (ID: {homeId})");
        processor.OnlineStatus = onlineStatus;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteProcessorAtHome(string homeId)
    {
        var processor = await GetProcessorByHomeId(homeId);
        if (processor == null) throw new ModelNotFoundException($"Processor does not exist in home (ID: {homeId})");
        _context.Processors.Remove(processor);
        await _context.SaveChangesAsync();
    }
}
