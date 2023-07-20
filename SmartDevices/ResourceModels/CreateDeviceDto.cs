using System.Text.Json.Serialization;
using smart_home_server.SmartDevices.Models;

namespace smart_home_server.SmartDevices.ResourceModels;

public class CreateDeviceDto<T> where T : class, new()
{
    public string RoomId { get; set; } = null!;

    public string Name { get; set; } = null!;

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public SubCategory SubCategory { get; set; }

    public T Capabilities { get; set; } = null!;
}