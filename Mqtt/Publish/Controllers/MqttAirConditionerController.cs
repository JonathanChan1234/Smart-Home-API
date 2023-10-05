
using Microsoft.AspNetCore.Http.HttpResults;
using MQTTnet.AspNetCore.Routing;
using MQTTnet.AspNetCore.Routing.Attributes;
using smart_home_server.Exceptions;
using smart_home_server.Home.Models;
using smart_home_server.Home.Services;
using smart_home_server.Mqtt.Client.Services;
using smart_home_server.SmartDevices.ResourceModels;
using smart_home_server.SmartDevices.SubDevices.AirConditioner.Models;
using smart_home_server.SmartDevices.SubDevices.AirConditioner.Service;

namespace smart_home_server.Mqtt.Publish.Controller;

[MqttController]
[MqttRoute("home/{homeId}/device/airConditioner")]
public class MqttAirConditionerController : MqttBaseController
{
    private readonly ILogger<MqttAirConditionerController> _logger;
    private readonly IAirConditionerService _airConditionerService;
    private readonly IHomeService _homeService;
    private readonly IMqttClientService _mqttClientService;

    public MqttAirConditionerController(
        ILogger<MqttAirConditionerController> logger,
        IAirConditionerService airConditionerService,
        IHomeService homeService,
        IMqttClientService mqttClientService
        )
    {
        _logger = logger;
        _airConditionerService = airConditionerService;
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
    public async Task UpdateAirConditionerStatus(
        string homeId,
        string deviceId,
        [FromPayload] UpdateDeviceStatusDto<AirConditionerProperties> dto)
    {
        try
        {
            var home = await AuthenticateUser(homeId);

            var ac = await _airConditionerService.FindAirConditionerById(home, deviceId)
                ?? throw new BadRequestException($"ac (id: {deviceId}) does not exist");
            await _airConditionerService.UpdateAirConditionerStatus(home, deviceId, dto.LastUpdatedAt, dto.Properties, dto.OnlineStatus);
            await Ok();
        }
        catch (Exception e)
        {
            _logger.LogDebug(e.Message);
            await BadMessage();
        }
    }

    [MqttRoute("{deviceId}/control")]
    public async Task HandleAirConditionerControl(
        string homeId,
        string deviceId,
        [FromPayload] UpdateDeviceStatusDto<AirConditionerProperties> dto)
    {
        try
        {
            if (dto is null) throw new BadRequestException("Invalid payload");
            var home = await AuthenticateUser(homeId);

            var ac = await _airConditionerService.FindAirConditionerById(home, deviceId)
                ?? throw new BadRequestException($"ac (id: {deviceId}) does not exist");
            await Ok();
        }
        catch (Exception e)
        {
            _logger.LogDebug(e.Message);
            await BadMessage();
        }
    }
}