namespace smart_home_server.Mqtt.Subscription;

public abstract class MqttSubscriptionBaseMiddleware
{
    [MqttSubscriptionContext]
    public MqttSubscriptionContext SubscriptionContext { get; set; } = null!;

    public abstract Task Handle();

    public virtual Task SubscriptionSuccess()
    {
        SubscriptionContext.MqttContext.ProcessSubscription = true;
        return Task.CompletedTask;
    }

    public virtual Task SubscriptionFailure()
    {
        SubscriptionContext.MqttContext.ProcessSubscription = false;
        return Task.CompletedTask;
    }
}