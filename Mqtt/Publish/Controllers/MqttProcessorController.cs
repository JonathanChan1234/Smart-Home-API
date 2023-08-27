using MQTTnet.AspNetCore.Routing;
using MQTTnet.AspNetCore.Routing.Attributes;
using smart_home_server.Exceptions;
using smart_home_server.Home.Services;
using smart_home_server.Mqtt.Client.Services;
using smart_home_server.Mqtt.Publish.Dto;
using smart_home_server.Processors.Service;

namespace smart_home_server.Mqtt.Publish.Controller;

[MqttController]
[MqttRoute("home/{homeId}/processor")]
public class MqttProcessorController : MqttBaseController
{
    private readonly ILogger<MqttProcessorController> _logger;
    private readonly IHomeService _homeService;
    private readonly IProcessorService _processorService;
    private readonly IMqttClientService _mqttClientService;

    public MqttProcessorController(
        IMqttClientService mqttClientService,
        IHomeService homeService,
        IProcessorService processorService,
        ILogger<MqttProcessorController> logger
    )
    {
        _mqttClientService = mqttClientService;
        _processorService = processorService;
        _homeService = homeService;
        _logger = logger;
    }

    private async Task ValidateClient(string homeId)
    {
        var clientId = MqttContext.ClientId;
        var validId = int.TryParse(clientId, out int cid);
        if (!validId) throw new BadRequestException($"invalid client id {clientId}");

        var home = await _homeService.GetHomeById(homeId, null);
        if (home == null) throw new BadRequestException($"home id {homeId} does not exist");

        var processor = await _processorService.GetProcessorByClientId(cid);
        // cannot find the mqtt client id
        if (processor == null) throw new BadRequestException($"processor id {clientId} does not exist");

        // incorrect topic
        if (home.ProcessorId != processor.Id) throw new BadRequestException($"This processor does not have right to access this home");
    }

    [MqttRoute("status")]
    public async Task UpdateProcessorStatus(
        string homeId,
        [FromPayload] UpdateProcessorStatusDto dto)
    {
        try
        {
            _logger.LogInformation($"Processor (clientId: {MqttContext.ClientId}) status updated to {(dto.OnlineStatus ? "online" : "offline")}");
            await ValidateClient(homeId);

            // update processor status
            await _processorService.UpdateProcessorOnlineStatus(homeId, dto.OnlineStatus);
            await Ok();
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            await BadMessage();
        }
    }
}