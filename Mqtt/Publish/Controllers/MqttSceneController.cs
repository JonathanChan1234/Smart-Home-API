
using MQTTnet.AspNetCore.Routing;
using smart_home_server.Exceptions;
using smart_home_server.Home.Models;
using smart_home_server.Home.Services;
using smart_home_server.Mqtt.Client.Services;
using smart_home_server.Scenes.Services;

[MqttController]
[MqttRoute("home/{homeId}/scene")]
public class MqttSceneController : MqttBaseController
{
    private readonly ILogger<MqttSceneController> _logger;
    private readonly ISceneService _sceneService;
    private readonly IHomeService _homeService;
    private readonly IMqttClientService _mqttClientSevice;

    public MqttSceneController(
        ILogger<MqttSceneController> logger,
        ISceneService sceneService,
        IHomeService homeService,
        IMqttClientService mqttClientService
        )
    {
        _logger = logger;
        _sceneService = sceneService;
        _homeService = homeService;
        _mqttClientSevice = mqttClientService;
    }

    private async Task<SmartHome> GetHomeFromParams(string homeId)
    {
        var home = await _homeService.GetHomeById(homeId, null);
        if (home == null) throw new ModelNotFoundException($"home (id: {homeId}) does not exist");
        return home;
    }

    private async Task ValidateClient(string homeId)
    {
        var clientId = MqttContext.ClientId;

        var validId = int.TryParse(MqttContext.ClientId, out int cid);
        if (!validId) throw new BadRequestException($"invalid client id {clientId}");

        var client = await _mqttClientSevice.FindMqttClientById(cid);
        // cannot find the mqtt client id
        if (client == null) throw new BadRequestException($"client id {clientId} does not exist");

        // incorrect topic
        if (homeId != client.HomeId.ToString()) throw new BadRequestException($"client does not have right to access this home");
    }

    [MqttRoute("{sceneId}")]
    public async Task UpdateLightStatus(string homeId, string sceneId)
    {
        try
        {
            await ValidateClient(homeId);

            var home = await GetHomeFromParams(homeId);
            var scene = await _sceneService.FindSceneById(homeId, sceneId);
            _logger.LogInformation($"scene action triggered: {scene?.Id.ToString() ?? "Invalid Scene"}");
            if (scene != null)
            {

                await Ok();
                return;
            }
        }
        catch (Exception e)
        {
            _logger.LogDebug(e.Message);
        }
        await BadMessage();
    }
}