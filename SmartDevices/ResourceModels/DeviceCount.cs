using System.Text.Json.Serialization;
using smart_home_server.SmartDevices.Models;

namespace smart_home_server.SmartDevices.ResourceModels;

public class DeviceCount
{
    public MainCategory MainCategory { get; set; }
    public int Count { get; set; }
}