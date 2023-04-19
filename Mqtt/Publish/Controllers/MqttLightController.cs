using System.ComponentModel.DataAnnotations;
using MQTTnet.AspNetCore.Routing;
using MQTTnet.AspNetCore.Routing.Attributes;
using smart_home_server.Devices.Services;
using smart_home_server.Mqtt.Client.Services;
using smart_home_server.Mqtt.Publish.Dto;

namespace smart_home_server.Mqtt.Publish.Controller;

[MqttController]
[MqttRoute("home/{homeId}/device/light")]
public class MqttLightController : MqttBaseController
{
    private readonly ILogger<MqttLightController> _logger;
    private readonly ILightService _lightService;
    private readonly IMqttClientService _mqttClientSevice;

    public MqttLightController(
        ILogger<MqttLightController> logger,
        ILightService lightService,
        IMqttClientService mqttClientService
        )
    {
        _logger = logger;
        _lightService = lightService;
        _mqttClientSevice = mqttClientService;
    }

    private bool ValidatePayload(LightPayloadDto dto)
    {
        ValidationContext context = new ValidationContext(dto);
        List<ValidationResult> validationResults = new List<ValidationResult>();
        return Validator.TryValidateObject(dto, context, validationResults);
    }

    private async Task<bool> ValidateClient(string clientId, string homeId)
    {
        var validId = int.TryParse(MqttContext.ClientId, out int cid);
        if (!validId) return false;

        var client = await _mqttClientSevice.FindMqttClientById(cid);
        // cannot find the mqtt client id
        if (client == null) return false;

        // incorrect topic
        if (homeId != client.HomeId.ToString()) return false;
        return true;
    }

    [MqttRoute("{deviceId}/status")]
    public async Task UpdateLightStatus(
        string homeId,
        string deviceId,
        [FromPayload] LightPayloadDto dto)
    {
        if (!ValidatePayload(dto) || !(await ValidateClient(MqttContext.ClientId, homeId)))
        {
            await BadMessage();
        }
        _logger.LogInformation($"status updated - home id: {homeId}, device id: {deviceId}, brightness: {dto.Brightness}, time: {dto.Time}");
        await _lightService.UpdateLightStatusInHome(homeId, deviceId, dto.Brightness, dto.Time);
        await Ok();
    }

    [MqttRoute("{deviceId}/control")]
    public async Task HandleLightControl(
        string homeId,
        string deviceId,
        [FromPayload] LightPayloadDto dto)
    {
        if (!ValidatePayload(dto) || !(await ValidateClient(MqttContext.ClientId, homeId)))
        {
            await BadMessage();
        }
        var light = await _lightService.FindLightInHome(homeId, deviceId);
        if (light == null || light?.StatusLastUpdatedAt > dto.Time)
            await BadMessage();

        // Broadcast the light control command to the controller
        await Ok();
    }
}