namespace smart_home_server.Scenes.ResourceModels;

public class DeviceActionDto<T> where T : class, new()
{
    public string DeviceId { get; set; } = null!;

    public T DeviceProperties { get; set; } = null!;
}