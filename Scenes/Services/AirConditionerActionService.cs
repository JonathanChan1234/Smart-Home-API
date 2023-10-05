using Microsoft.EntityFrameworkCore;
using smart_home_server.Db;
using smart_home_server.Exceptions;
using smart_home_server.Home.Models;
using smart_home_server.Scenes.Models;
using smart_home_server.SmartDevices.SubDevices.AirConditioner.Models;
using smart_home_server.SmartDevices.SubDevices.AirConditioner.Service;
using smart_home_server.Utils;

namespace smart_home_server.Scenes.Services;

public interface IAirConditionerActionService
{
    public Task<SceneAction> CreateAirConditionerAction(SmartHome home, Scene scene, string deviceId, AirConditionerProperties properties);
    public Task EditAirConditionerAction(SmartHome home, Scene scene, string actionId, AirConditionerProperties properties);
}

public class AirConditionerActionService : IAirConditionerActionService
{
    public AirConditionerActionService(
        AppDbContext context,
        ISceneActionService sceneActionService,
        IAirConditionerService airConditionerService
    )
    {
        _context = context;
        _sceneActionService = sceneActionService;
        _airConditionerService = airConditionerService;
    }

    private readonly AppDbContext _context;
    private readonly ISceneActionService _sceneActionService;
    private readonly IAirConditionerService _airConditionerService;

    public async Task<SceneAction> CreateAirConditionerAction(
        SmartHome home,
        Scene scene,
        string deviceId,
        AirConditionerProperties properties
    )
    {
        var ac = await _airConditionerService.FindAirConditionerById(home, deviceId)
            ?? throw new ModelNotFoundException($"AC (device id: {deviceId}) does not exist");

        var capabilties = ac.Capabilities.ToObject<AirConditionerCapabilities>();
        if (!CheckAirConditionerCapabilties(properties, capabilties))
            throw new BadRequestException("Please check the ac's capabilties. The ac action contains invalid properties");

        // Check if there is already action with the same device in the same scene
        var existingAction = await _sceneActionService.FindSceneActionByDeviceId(scene.Id.ToString(), ac.Id.ToString());
        if (existingAction != null) throw new BadRequestException($"Action with device (id: {ac.Id}) already exists");

        SceneAction sceneAction = new()
        {
            Device = ac,
            Scene = scene,
            Action = properties.ToDict(),
            Description = ""
        };

        _context.SceneActions.Add(sceneAction);
        scene.Actions.Add(sceneAction);
        await _context.SaveChangesAsync();
        return sceneAction;
    }

    public async Task EditAirConditionerAction(
        SmartHome home,
        Scene scene,
        string actionId,
        AirConditionerProperties properties
    )
    {
        // Check if the action id exists
        var action = await _sceneActionService.FindSceneActionById(scene.Id.ToString(), actionId)
            ?? throw new ModelNotFoundException($"Action (id: {actionId}) does not exist");

        // Check if the light still exists. If yes, check its dimmable compatibility
        var ac = await _airConditionerService.FindAirConditionerById(home, action.DeviceId.ToString())
            ?? throw new BadRequestException($"AC (id: {action.DeviceId}) does not exist");

        var capabilties = ac.Capabilities.ToObject<AirConditionerCapabilities>();
        if (!CheckAirConditionerCapabilties(properties, capabilties))
            throw new BadRequestException("Please check the light's capabilties. The light action contains invalid properties");

        action.Action = properties.ToDict();
        await _context.SaveChangesAsync();
    }

    private bool CheckAirConditionerCapabilties(
        AirConditionerProperties properties,
        AirConditionerCapabilities capabilities)
    {
        switch (properties.OperationMode)
        {
            case OperationMode.Cool:
                if (!capabilities.CoolMode) throw new BadRequestException("This AC does not support cool mode");
                break;
            case OperationMode.Heat:
                if (!capabilities.HeatMode) throw new BadRequestException("This AC does not support heat mode");
                break;
            case OperationMode.Fan:
                if (!capabilities.FanMode) throw new BadRequestException("This AC does not support fan mode");
                break;
            case OperationMode.Dry:
                if (!capabilities.DryMode) throw new BadRequestException("This AC does not support dry mode");
                break;
            case OperationMode.Auto:
                if (!capabilities.AutoMode) throw new BadRequestException("This AC does not support auto mode");
                break;
            case null:
                break;
        }
        switch (properties.FanSpeed)
        {
            case FanSpeed.Quiet:
                if (!capabilities.QuietFanSpeed) throw new BadRequestException("This AC does not support quiet fan speed");
                break;
            case FanSpeed.Low:
                if (!capabilities.LowFanSpeed) throw new BadRequestException("This AC does not support low fan speed");
                break;
            case FanSpeed.Medium:
                if (!capabilities.MediumFanSpeed) throw new BadRequestException("This AC does not support medium fan speed");
                break;
            case FanSpeed.High:
                if (!capabilities.HighFanSpeed) throw new BadRequestException("This AC does not support high fan speed");
                break;
            case FanSpeed.Top:
                if (!capabilities.TopFanSpeed) throw new BadRequestException("This AC does not support top fan speed");
                break;
            case FanSpeed.Auto:
                if (!capabilities.AutoFanSpeed) throw new BadRequestException("This AC does not support auto fan speed");
                break;
            case null:
                break;
        }
        if (properties.SetTemperature > capabilities.SetTemperatureHighEnd ||
            properties.SetTemperature < capabilities.SetTemperatureLowEnd)
        {
            throw new BadRequestException($"Set Temperature must be between ${capabilities.SetTemperatureLowEnd} and ${capabilities.SetTemperatureLowEnd}");
        }
        return true;
    }
}