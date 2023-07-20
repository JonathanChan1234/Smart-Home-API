using Microsoft.EntityFrameworkCore;
using smart_home_server.Db;
using smart_home_server.Exceptions;
using smart_home_server.Home.Models;
using smart_home_server.Scenes.Models;
using smart_home_server.SmartDevices.SubDevices.Lights.Models;
using smart_home_server.SmartDevices.SubDevices.Lights.Service;
using smart_home_server.Utils;

namespace smart_home_server.Scenes.Services;

public interface ILightActionService
{
    Task<SceneAction> CreateLightAction(SmartHome home, Scene scene, string deviceId, LightProperties properties);
    Task EditLightAction(SmartHome home, Scene scene, String actionId, LightProperties properties);
}

public class LightActionService : ILightActionService
{
    private readonly AppDbContext _context;
    private readonly ISmartLightService _lightService;
    private readonly ISceneActionService _sceneActionService;

    public LightActionService(
        AppDbContext context,
        ISmartLightService lightService,
        ISceneActionService sceneActionService)
    {
        _sceneActionService = sceneActionService;
        _lightService = lightService;
        _context = context;
    }

    public async Task<SceneAction> CreateLightAction(SmartHome home, Scene scene, string deviceId, LightProperties lightAction)
    {
        var light = await _lightService.FindLightById(home, deviceId);
        if (light == null) throw new ModelNotFoundException($"light (device id: {deviceId}) does not exist");

        var capabilties = light.Capabilities.ToObject<LightCapabilities>();
        if (!CheckLightCapabilities(lightAction, capabilties))
            throw new BadRequestException("Please check the light's capabilties. The light action contains invalid properties");

        // Check if there is already action with the same device in the same scene
        var existingAction = await _sceneActionService.FindSceneActionByDeviceId(scene.Id.ToString(), light.Id.ToString());
        if (existingAction != null) throw new BadRequestException($"Action with device (id: {light.Id}) already exists");

        SceneAction sceneAction = new SceneAction
        {
            Device = light,
            Scene = scene,
            Action = lightAction.ToDict<LightProperties>(),
            Description = ""
        };

        _context.SceneActions.Add(sceneAction);
        scene.Actions.Add(sceneAction);
        await _context.SaveChangesAsync();
        return sceneAction;
    }

    public async Task EditLightAction(SmartHome home, Scene scene, string actionId, LightProperties lightAction)
    {
        // Check if the action id exists
        var action = await _context.SceneActions.FirstOrDefaultAsync(a => a.Id.ToString() == actionId);
        if (action == null) throw new ModelNotFoundException($"Action (id: {actionId}) does not exist");

        // Check if the light still exists. If yes, check its dimmable compatibility
        var light = await _lightService.FindLightById(home, action.DeviceId.ToString());
        if (light == null) throw new BadRequestException($"Light (id: {action.DeviceId.ToString()}) does not exist");

        var capabilties = light.Capabilities.ToObject<LightCapabilities>();
        if (!CheckLightCapabilities(lightAction, capabilties))
            throw new BadRequestException("Please check the light's capabilties. The light action contains invalid properties");

        action.Action = lightAction.ToDict<LightProperties>();
        await _context.SaveChangesAsync();
    }

    private bool CheckLightCapabilities(LightProperties properties, LightCapabilities capabilities)
    {
        // For non-dimmable light, brightness can either be null or 0/100
        if (!capabilities.Dimmable
            && properties.Brightness != null
            && (properties.Brightness != 0 && properties.Brightness != 100))
            return false;

        // For light not supporting color temperature change, color temperature must be null
        if (!capabilities.HasColorTemperature && properties.ColorTemperature != null)
            return false;
        return true;
    }
}