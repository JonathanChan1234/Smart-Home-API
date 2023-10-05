using smart_home_server.Exceptions;
using smart_home_server.Home.Models;
using smart_home_server.Home.Services;
using smart_home_server.Mqtt.Client.Services;

public class MqttAuthHelper
{
    public static async Task<SmartHome> GetHomeFromParams(IHomeService homeService, string homeId)
    {
        var home = await homeService.GetHomeById(homeId, null)
            ?? throw new ModelNotFoundException($"home (id: {homeId}) does not exist");
        return home;
    }

    public static async Task ValidateClient(IMqttClientService mqttClientService, string clientId, string homeId)
    {
        var validId = int.TryParse(clientId, out int cid);
        if (!validId) throw new BadRequestException($"invalid client id {clientId}");

        var client = await mqttClientService.FindMqttClientById(cid)
            ?? throw new BadRequestException($"client id {clientId} does not exist");

        // incorrect topic
        if (homeId != client.HomeId.ToString()) throw new BadRequestException($"client does not have right to access this home");
    }
}