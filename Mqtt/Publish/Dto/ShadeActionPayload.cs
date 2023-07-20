
using System.ComponentModel.DataAnnotations;

namespace smart_home_server.Mqtt.Publish.Dto;
public class ShadeActionPayload
{
    public string Action { get; set; } = null!;

    [DataType(DataType.DateTime)]
    public DateTime Time { get; set; }
}