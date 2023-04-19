using smart_home_server.Mqtt.Client.Services;

namespace smart_home_server.Mqtt.Subscription;

public class MqttSubscriptionMiddleware : MqttSubscriptionBaseMiddleware
{
    private readonly ILogger<MqttSubscriptionMiddleware> _logger;
    private readonly IMqttClientService _mqttClientService;

    public MqttSubscriptionMiddleware(
        ILogger<MqttSubscriptionMiddleware> logger,
        IMqttClientService mqttClientService
    )
    {
        _logger = logger;
        _mqttClientService = mqttClientService;
    }

    public override async Task Handle()
    {
        try
        {
            var mqttClient = await _mqttClientService.FindMqttClientById(int.Parse(SubscriptionContext.MqttContext.ClientId));
            if (mqttClient == null) throw new NullReferenceException("Mqtt client id not found");

            var topic = SubscriptionContext.MqttContext.TopicFilter;
            var levels = topic.Topic.Split('/');

            if (levels.Length < 3) throw new Exception($"Invalid topic {topic}, topic must contain at least 3 levels");
            if (levels[0] != "home") throw new Exception($"Invalid topic {topic}, the first level must be equal to home");
            if (levels[1] != mqttClient.HomeId.ToString()) throw new Exception($"Invalid topic {topic}, the second level must be equal to client home id");

            await SubscriptionSuccess();
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            await SubscriptionFailure();
        }
    }
}