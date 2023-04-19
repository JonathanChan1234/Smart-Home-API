using MQTTnet.Server;

namespace smart_home_server.Mqtt.Subscription;

public class MqttSubscriptionContext
{
    public InterceptingSubscriptionEventArgs MqttContext { get; set; } = null!;
    public MqttServer MqttServer { get; set; } = null!;
}