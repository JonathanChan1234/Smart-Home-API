using System.Reflection;
using MQTTnet.Server;
using smart_home_server.Mqtt.Authentication;

namespace smart_home_server.Mqtt.Subscription;

public class MqttSubscriptionHandler
{
    private readonly ILogger<MqttSubscriptionHandler> _logger;
    private readonly IMqttAuthTypeActivatorCache _typeActivator;

    public MqttSubscriptionHandler(
        ILogger<MqttSubscriptionHandler> logger,
        IMqttAuthTypeActivatorCache typeActivator
    )
    {
        _logger = logger;
        _typeActivator = typeActivator;
    }

    public async Task OnInterceptingSubscription(
        IServiceProvider svcProvider,
        InterceptingSubscriptionEventArgs args
    )
    {
        args.ProcessSubscription = true;
        using (var scope = svcProvider.CreateScope())
        {
            Type type = typeof(MqttSubscriptionMiddleware);
            var classInstance = _typeActivator.CreateInstance<object>(scope.ServiceProvider, type);
            var activateProperties = type.GetRuntimeProperties()
                .Where((property) =>
                {
                    return property.SetMethod != null &&
                        !property.SetMethod.IsStatic &&
                        property.IsDefined(typeof(MqttSubscriptionContextAttribute)) &&
                        property.GetIndexParameters().Length == 0;
                })
                .ToArray();
            var subscriptionContext = new MqttSubscriptionContext()
            {
                MqttContext = args,
                MqttServer = scope.ServiceProvider.GetRequiredService<MqttServer>(),
            };
            for (int i = 0; i < activateProperties.Length; i++)
            {
                PropertyInfo property = activateProperties[i];
                property.SetValue(classInstance, subscriptionContext);
            }

            MethodInfo? subscriptionHandler = type.GetMethod("Handle");
            if (subscriptionHandler == null) throw new NullReferenceException("MQTT subscription does not contain Handle method");
            if (subscriptionHandler.ReturnType != typeof(Task))
            {
                throw new InvalidOperationException($"Unsupported action type {subscriptionHandler.ReturnType}");
            }
            var task = (Task?)subscriptionHandler.Invoke(classInstance, null);
            if (task == null)
            {
                throw new NullReferenceException($"{subscriptionHandler.DeclaringType}.{subscriptionHandler.Name} return null instead of task");
            }
            await task.ConfigureAwait(false);
        }
    }
}