using System.ComponentModel.DataAnnotations;
using smart_home_server.SmartDevices.SubDevices.Lights.Models;

namespace smart_home_server.Mqtt.Publish.Dto;
public class LightPayloadDto
{
    public LightProperties Properties { get; set; } = null!;

    [DataType(DataType.DateTime)]
    public DateTime Time { get; set; }
}