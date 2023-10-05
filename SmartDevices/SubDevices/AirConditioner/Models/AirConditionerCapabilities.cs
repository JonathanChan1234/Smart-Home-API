namespace smart_home_server.SmartDevices.SubDevices.AirConditioner.Models;

public class AirConditionerCapabilities
{
    public bool QuietFanSpeed { get; set; }
    public bool LowFanSpeed { get; set; }
    public bool MediumFanSpeed { get; set; }
    public bool HighFanSpeed { get; set; }
    public bool TopFanSpeed { get; set; }
    public bool AutoFanSpeed { get; set; }
    public bool FanMode { get; set; }
    public bool HeatMode { get; set; }
    public bool CoolMode { get; set; }
    public bool DryMode { get; set; }
    public bool AutoMode { get; set; }
    public int SetTemperatureHighEnd { get; set; }
    public int SetTemperatureLowEnd { get; set; }
    public bool ShowRoomTemperature { get; set; } = false;
}