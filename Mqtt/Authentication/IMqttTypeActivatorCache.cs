namespace smart_home_server.Mqtt.Authentication
{
    public interface IMqttAuthTypeActivatorCache
    {
        TInstance CreateInstance<TInstance>(IServiceProvider serviceProvider, Type implementationType);
    }
}