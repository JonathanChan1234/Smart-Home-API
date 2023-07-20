using smart_home_server.Db;
using smart_home_server.Exceptions;
using smart_home_server.Home.Models;
using smart_home_server.Scenes.Models;
using smart_home_server.SmartDevices.SubDevices.Shades.Models;
using smart_home_server.SmartDevices.SubDevices.Shades.Services;
using smart_home_server.Utils;

namespace smart_home_server.Scenes.Services;

public interface IShadeActionService
{
    Task<SceneAction> CreateShadeAction(SmartHome home, Scene scene, string deviceId, ShadeAction properties);
    Task EditShadeAction(SmartHome home, Scene scene, string actionId, ShadeAction properties);
}

public class ShadeActionService : IShadeActionService
{
    private readonly AppDbContext _context;
    private readonly ISmartShadeService _shadeService;
    private readonly ISceneActionService _sceneActionService;

    public ShadeActionService(
        AppDbContext context,
        ISmartShadeService shadeService,
        ISceneActionService sceneActionService)
    {
        _shadeService = shadeService;
        _sceneActionService = sceneActionService;
        _context = context;
    }

    public async Task<SceneAction> CreateShadeAction(SmartHome home, Scene scene, string deviceId, ShadeAction shadeAction)
    {
        var shade = await _shadeService.FindShadeById(home, deviceId);
        if (shade == null) throw new ModelNotFoundException($"shade (id: {deviceId}) does not exist");

        if (!CheckShadeCapabilties(shadeAction, shade.Capabilities.ToObject<ShadeCapabilities>()))
            throw new BadRequestException("The shade action does not match its capabilites");

        var existingShadeAction = await _sceneActionService.FindSceneActionByDeviceId(scene.Id.ToString(), shade.Id.ToString());
        if (existingShadeAction != null) throw new BadRequestException($"Scene action with device (id: {shade.Id.ToString()}) already exist");

        SceneAction action = new SceneAction
        {
            Device = shade,
            Scene = scene,
            Action = shadeAction.ToDict<ShadeAction>(),
            Description = ""
        };

        _context.SceneActions.Add(action);
        scene.Actions.Add(action);
        await _context.SaveChangesAsync();
        return action;
    }

    public async Task EditShadeAction(SmartHome home, Scene scene, string actionId, ShadeAction shadeAction)
    {
        var action = await _sceneActionService.FindSceneActionById(scene.Id.ToString(), actionId);
        if (action == null) throw new ModelNotFoundException();

        var shade = await _shadeService.FindShadeById(home, action.DeviceId.ToString());
        if (shade == null) throw new ModelNotFoundException($"shade (id: {action.DeviceId.ToString()}) does not exist");

        if (!CheckShadeCapabilties(shadeAction, shade.Capabilities.ToObject<ShadeCapabilities>()))
            throw new BadRequestException("The shade action does not match its capabilites");

        action.Action = shadeAction.ToDict<ShadeAction>();
        await _context.SaveChangesAsync();
    }

    private bool CheckShadeCapabilties(ShadeAction shadeAction, ShadeCapabilities capabilities)
    {
        if (capabilities.HasLevel && shadeAction.Level == null) return false;
        if (!capabilities.HasLevel && shadeAction.ActionType == null) return false;
        return true;
    }
}