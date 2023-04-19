using MQTTnet.Server;

namespace smart_home_server.Mqtt.Authentication;

public class MqttAuthenticationContext
{
    public ValidatingConnectionEventArgs MqttContext { get; set; } = null!;
    public MqttServer MqttServer { get; set; } = null!;
}