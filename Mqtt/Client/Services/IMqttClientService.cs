
using smart_home_server.Auth.Models;
using smart_home_server.Home.Models;
using smart_home_server.Mqtt.Client.Models;

namespace smart_home_server.Mqtt.Client.Services;

public interface IMqttClientService
{
    Task<MqttClient?> FindMqttClientById(int clientId);
    Task<MqttClient> RegisterMqttClient(ApplicationUser user, SmartHome home);
    Task RevokeAllMqttClients(ApplicationUser user, SmartHome home);
}