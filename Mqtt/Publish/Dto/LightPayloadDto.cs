using System.ComponentModel.DataAnnotations;

namespace smart_home_server.Mqtt.Publish.Dto;
public class LightPayloadDto
{
    [Range(0, 100)]
    public int Brightness { get; set; }

    [DataType(DataType.DateTime)]
    public DateTime Time { get; set; }
}