
using System.ComponentModel.DataAnnotations;

namespace smart_home_server.Mqtt.Publish.Dto;
public class ShadeLevelPayload
{
    [Range(0, 100)]
    public int Level { get; set; }

    [DataType(DataType.DateTime)]
    public DateTime Time { get; set; }
}