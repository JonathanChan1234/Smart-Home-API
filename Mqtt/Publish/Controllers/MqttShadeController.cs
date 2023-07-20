
using System.ComponentModel.DataAnnotations;
using MQTTnet.AspNetCore.Routing;
using MQTTnet.AspNetCore.Routing.Attributes;
using smart_home_server.Exceptions;
using smart_home_server.Home.Models;
using smart_home_server.Home.Services;
using smart_home_server.Mqtt.Client.Services;
using smart_home_server.Mqtt.Publish.Dto;
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
    private readonly IMqttClientService _mqttClientSevice;

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
        _mqttClientSevice = mqttClientService;
    }

    private bool ValidatePayload<T>(T dto)
    {
        if (dto == null) return false;
        ValidationContext context = new ValidationContext(dto);
        List<ValidationResult> validationResults = new List<ValidationResult>();
        return Validator.TryValidateObject(dto, context, validationResults);
    }

    private async Task ValidateClient(string homeId)
    {
        var clientId = MqttContext.ClientId;
        var validId = int.TryParse(clientId, out int cid);
        if (!validId) throw new BadRequestException($"invalid client id {clientId}");

        var client = await _mqttClientSevice.FindMqttClientById(cid);
        // cannot find the mqtt client id
        if (client == null) throw new BadRequestException($"client id {clientId} does not exist");

        // incorrect topic
        if (homeId != client.HomeId.ToString()) throw new BadRequestException($"client does not have right to access this home");
    }

    private async Task<SmartHome> GetHomeFromParams(string homeId)
    {
        var home = await _homeService.GetHomeById(homeId, null);
        if (home == null) throw new ModelNotFoundException($"home (id: {homeId}) does not exist");
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
            await ValidateClient(homeId);

            var home = await GetHomeFromParams(homeId);
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
        [FromPayload] ShadeActionPayload dto)
    {
        try
        {
            await ValidateClient(homeId);
            var home = await GetHomeFromParams(homeId);
            var shade = await _shadeService.FindShadeById(home, deviceId);
            if (shade == null || shade?.StatusLastUpdatedAt > dto.Time)
                throw new BadRequestException("control command date expired");
            await Ok();
        }
        catch (Exception e)
        {
            _logger.LogDebug(e.ToString());
            await BadMessage();
        }
    }
}