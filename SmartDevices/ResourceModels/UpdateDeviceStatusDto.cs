namespace smart_home_server.SmartDevices.ResourceModels;

public class UpdateDeviceStatusDto<T> where T : class, new()
{
    public DateTime LastUpdatedAt { get; set; }
    public T? Properties { get; set; }
    public bool? OnlineStatus { get; set; }
}