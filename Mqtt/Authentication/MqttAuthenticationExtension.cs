using MQTTnet.Server;
using Microsoft.IdentityModel.Tokens;
using System.Reflection;
using smart_home_server.Mqtt.Subscription;

namespace smart_home_server.Mqtt.Authentication;

public static class MqttAuthenticationExtension
{
    public static IServiceCollection AddMqttAuthentication(this IServiceCollection services, Assembly[] fromAssemblies = null)
    {
        services.AddSingleton<IMqttAuthTypeActivatorCache>(new MqttAuthTypeActivatorCache());
        services.AddSingleton<MqttAuthenticationHandler>();
        services.AddSingleton<MqttSubscriptionHandler>();
        return services;
    }

    public static void WithAuthentication(this MqttServer server, IServiceProvider svcProvider)
    {
        var tokenValidationParameters = svcProvider.GetRequiredService<TokenValidationParameters>();
        var authenticationHandler = svcProvider.GetRequiredService<MqttAuthenticationHandler>();
        var subscriptionHandler = svcProvider.GetRequiredService<MqttSubscriptionHandler>();
        server.ValidatingConnectionAsync += (args) => authenticationHandler.OnValidatingConnection(svcProvider, args);
        server.InterceptingSubscriptionAsync += (args) => subscriptionHandler.OnInterceptingSubscription(svcProvider, args);
    }
}