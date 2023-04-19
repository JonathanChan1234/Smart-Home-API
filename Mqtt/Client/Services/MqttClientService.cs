using Microsoft.EntityFrameworkCore;
using smart_home_server.Auth.Models;
using smart_home_server.Db;
using smart_home_server.Home.Models;
using smart_home_server.Mqtt.Client.Models;

namespace smart_home_server.Mqtt.Client.Services;


public class MqttClientService : IMqttClientService
{
    private readonly AppDbContext _context;

    public MqttClientService(
        AppDbContext context
    )
    {
        _context = context;
    }

    public async Task<MqttClient> RegisterMqttClient(
        ApplicationUser user,
        SmartHome home
    )
    {
        var mqttClient = new MqttClient()
        {
            User = user,
            Home = home,
            Revoked = false,
            CreatedAt = DateTime.Now
        };
        _context.MqttClients.Add(mqttClient);
        await _context.SaveChangesAsync();
        return mqttClient;
    }

    public Task<MqttClient?> FindMqttClientById(
        int clientId
    )
    {
        return _context.MqttClients.FirstOrDefaultAsync(m => m.ClientId == clientId);
    }

    /// <summary>
    /// This method will revoke all the mqtt client id associated with this home
    /// Used when the user delete the home
    /// </summary>
    public Task RevokeAllMqttClients(ApplicationUser user, SmartHome home)
    {
        return _context
            .MqttClients
            .Where(m => m.HomeId == home.Id)
            .ExecuteUpdateAsync(m => m.SetProperty(c => c.Revoked, c => true));
    }
}