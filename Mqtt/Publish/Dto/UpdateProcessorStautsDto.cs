namespace smart_home_server.Mqtt.Publish.Dto;

public class UpdateProcessorStatusDto
{
    public bool OnlineStatus { get; set; }
    public DateTime LastUpdatedAt { get; set; }
}