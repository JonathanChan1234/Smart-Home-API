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
    private readonly IMqttClientService _mqttClientService;

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
        _mqttClientService = mqttClientService;
    }

    private async Task<SmartHome> AuthenticateUser(string homeId)
    {
        await MqttAuthHelper.ValidateClient(_mqttClientService, MqttContext.ClientId, homeId);
        var home = await MqttAuthHelper.GetHomeFromParams(_homeService, homeId);
        return home;
    }

    [MqttRoute("{deviceId}/status")]
    public async Task UpdateLightStatus(
        string homeId,
        string deviceId,
        [FromPayload] UpdateDeviceStatusDto<LightProperties> dto)
    {
        try
        {
            var home = await AuthenticateUser(homeId);

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
            if (dto is null) throw new BadRequestException("Invalid payload");
            var home = await AuthenticateUser(homeId);

            var light = await _lightService.FindLightById(home, deviceId)
                ?? throw new BadRequestException("control command date expired");
            await Ok();
        }
        catch (Exception e)
        {
            _logger.LogDebug(e.Message);
            await BadMessage();
        }
    }
}