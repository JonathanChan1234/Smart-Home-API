
namespace smart_home_server.SmartDevices.SubDevices.AirConditioner.Models;

public class AirConditionerProperties
{
    public bool? Power { get; set; }
    public FanSpeed? FanSpeed { get; set; }
    public OperationMode? OperationMode { get; set; }
    public double? SetTemperature { get; set; }
    public double? RoomTemperature { get; set; }
}