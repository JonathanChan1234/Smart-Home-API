namespace smart_home_server.SmartDevices.ResourceModels;

public class UpdateDeviceDto
{
    public string? RoomId { get; set; }
    public string Name { get; set; } = null!;
}