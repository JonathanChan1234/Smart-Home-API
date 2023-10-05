
using System.ComponentModel.DataAnnotations;
using MQTTnet.AspNetCore.Routing;
using MQTTnet.AspNetCore.Routing.Attributes;
using smart_home_server.Exceptions;
using smart_home_server.Home.Models;
using smart_home_server.Home.Services;
using smart_home_server.Mqtt.Client.Services;
using smart_home_server.Scenes.Models;
using smart_home_server.SmartDevices.ResourceModels;
using smart_home_server.SmartDevices.SubDevices.Shades.Models;
using smart_home_server.SmartDevices.SubDevices.Shades.Services;

namespace smart_home_server.Mqtt.Publish.Controller;

[MqttController]
[MqttRoute("home/{homeId}/device/shade")]
public class MqttShadeController : MqttBaseController
{
    private readonly ILogger<MqttShadeController> _logger;
    private readonly ISmartShadeService _shadeService;
    private readonly IHomeService _homeService;
    private readonly IMqttClientService _mqttClientService;

    public MqttShadeController(
        ILogger<MqttShadeController> logger,
        ISmartShadeService shadeService,
        IHomeService homeService,
        IMqttClientService mqttClientService
        )
    {
        _logger = logger;
        _shadeService = shadeService;
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
    public async Task UpdateShadeStatus(
        string homeId,
        string deviceId,
        [FromPayload] UpdateDeviceStatusDto<ShadeProperties> dto)
    {
        try
        {
            var home = await AuthenticateUser(homeId);

            await _shadeService.UpdateShadeStatus(home, deviceId, dto.LastUpdatedAt, dto.Properties, dto.OnlineStatus);
            await Ok();
        }
        catch (Exception e)
        {
            _logger.LogDebug(e.ToString());
            await BadMessage();
        }
    }

    [MqttRoute("{deviceId}/control")]
    public async Task HandleShadeActionControl(
        string homeId,
        string deviceId,
        [FromPayload] UpdateDeviceStatusDto<ShadeAction> dto)
    {
        try
        {
            var home = await AuthenticateUser(homeId);
            var shade = await _shadeService.FindShadeById(home, deviceId)
                ?? throw new BadRequestException("control command date expired");
            await Ok();
        }
        catch (Exception e)
        {
            _logger.LogDebug(e.ToString());
            await BadMessage();
        }
    }
}