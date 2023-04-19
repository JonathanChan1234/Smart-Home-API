using System.Reflection;
using MQTTnet.Server;

namespace smart_home_server.Mqtt.Authentication;

public class MqttAuthenticationHandler
{
    private readonly ILogger<MqttAuthenticationHandler> _logger;
    private readonly IMqttAuthTypeActivatorCache _typeActivator;

    public MqttAuthenticationHandler(
        ILogger<MqttAuthenticationHandler> logger,
        IMqttAuthTypeActivatorCache typeActivator)
    {
        _logger = logger;
        _typeActivator = typeActivator;
    }

    public async Task OnValidatingConnection(
        IServiceProvider svcProvider,
        ValidatingConnectionEventArgs args)
    {
        _logger.LogInformation("validating connection");
        args.ReasonCode = MQTTnet.Protocol.MqttConnectReasonCode.Success;
        using (var scope = svcProvider.CreateScope())
        {
            Type type = typeof(MqttAuthenticationMiddlware);
            var classInstance = _typeActivator.CreateInstance<object>(scope.ServiceProvider, type);
            var activateProperties = type.GetRuntimeProperties()
                .Where((property) =>
                {
                    return property.SetMethod != null &&
                        !property.SetMethod.IsStatic &&
                        property.IsDefined(typeof(MqttAuthenticationContextAttribute)) &&
                        property.GetIndexParameters().Length == 0;
                })
                .ToArray();
            var authenticationContext = new MqttAuthenticationContext()
            {
                MqttContext = args,
                MqttServer = scope.ServiceProvider.GetRequiredService<MqttServer>(),
            };
            for (int i = 0; i < activateProperties.Length; i++)
            {
                PropertyInfo property = activateProperties[i];
                property.SetValue(classInstance, authenticationContext);
            }

            MethodInfo? authenticationHandler = type.GetMethod("Handle");
            if (authenticationHandler == null) throw new NullReferenceException("MQTT authentication does not contain Handle method");
            if (authenticationHandler.ReturnType != typeof(Task))
            {
                throw new InvalidOperationException($"Unsupported action type {authenticationHandler.ReturnType}");
            }
            var task = (Task?)authenticationHandler.Invoke(classInstance, null);
            if (task == null)
            {
                throw new NullReferenceException($"{authenticationHandler.DeclaringType}.{authenticationHandler.Name} return null instead of task");
            }
            await task.ConfigureAwait(false);
        }
    }
}