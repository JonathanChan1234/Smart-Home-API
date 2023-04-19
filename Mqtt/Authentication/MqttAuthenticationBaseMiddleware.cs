
namespace smart_home_server.Mqtt.Authentication;

public abstract class MqttAuthenticationBaseMiddlware
{
    [MqttAuthenticationContextAttribute]
    public MqttAuthenticationContext AuthenticationContext { get; set; } = null!;

    /// <summary>
    /// override to handle the user authentication process
    /// </summary> ///
    public abstract Task Handle();

    public virtual Task AuthSuccess()
    {
        AuthenticationContext.MqttContext.ReasonCode = MQTTnet.Protocol.MqttConnectReasonCode.Success;
        return Task.CompletedTask;
    }

    public virtual Task AuthFailure()
    {
        AuthenticationContext.MqttContext.ReasonCode = MQTTnet.Protocol.MqttConnectReasonCode.BadUserNameOrPassword;
        return Task.CompletedTask;
    }
}