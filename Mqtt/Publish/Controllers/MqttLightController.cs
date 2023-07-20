using MQTTnet.AspNetCore.Routing;
using MQTTnet.AspNetCore.Routing.Attributes;
using smart_home_server.Exceptions;
using smart_home_server.Home.Models;
using smart_home_server.Home.Services;
using smart_home_server.Mqtt.Client.Services;
using smart_home_server.SmartDevices.ResourceModels;
using smart_home_server.SmartDevices.SubDevices.Lights.Models;
using smart_home_server.SmartDevices.SubDevices.Lights.Service;

namespace smart_home_server.Mqtt.Publish.Controller;

[MqttController]
[MqttRoute("home/{homeId}/device/light")]
public class MqttLightController : MqttBaseController
{
    private readonly ILogger<MqttLightController> _logger;
    private readonly ISmartLightService _lightService;
    private readonly IHomeService _homeService;
    private readonly IMqttClientService _mqttClientSevice;

    public MqttLightController(
        ILogger<MqttLightController> logger,
        ISmartLightService lightService,
        IHomeService homeService,
        IMqttClientService mqttClientService
        )
    {
        _logger = logger;
        _lightService = lightService;
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

    [MqttRoute("{deviceId}/status")]
    public async Task UpdateLightStatus(
        string homeId,
        string deviceId,
        [FromPayload] UpdateDeviceStatusDto<LightProperties> dto)
    {
        try
        {
            await ValidateClient(homeId);

            var home = await GetHomeFromParams(homeId);
            await _lightService.UpdateLightStatus(home, deviceId, dto.LastUpdatedAt, dto.Properties, dto.OnlineStatus);
            await Ok();
        }
        catch (Exception e)
        {
            _logger.LogDebug(e.Message);
            await BadMessage();
        }
    }

    [MqttRoute("{deviceId}/control")]
    public async Task HandleLightControl(
        string homeId,
        string deviceId,
        [FromPayload] UpdateDeviceStatusDto<LightProperties> dto)
    {
        try
        {
            await ValidateClient(homeId);
            var home = await GetHomeFromParams(homeId);
            var light = await _lightService.FindLightById(home, deviceId);

            if (light == null || light?.StatusLastUpdatedAt > dto.LastUpdatedAt)
                throw new BadRequestException("control command date expired");
            await Ok();
        }
        catch (Exception e)
        {
            _logger.LogDebug(e.Message);
            await BadMessage();
        }
    }
}