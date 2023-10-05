using Microsoft.EntityFrameworkCore;
using smart_home_server.Db;
using smart_home_server.Exceptions;
using smart_home_server.Home.Models;
using smart_home_server.Scenes.Models;
using smart_home_server.SmartDevices.Models;

namespace smart_home_server.Scenes.Services;

public interface ISceneActionService
{
    Task<List<SceneAction>> FindSceneActions(string sceneId);
    Task DeleteSceneAction(Scene scene, string actionId);
    Task<SceneAction?> FindSceneActionById(string sceneId, string actionId);
    Task<SceneAction?> FindSceneActionByDeviceId(string sceneId, string deviceId);
    Task<List<SmartDevice>> FindAvailableSmartDevices(SmartHome home, Scene scene);
}

public class SceneActionService : ISceneActionService
{
    private readonly AppDbContext _context;

    public SceneActionService(
        AppDbContext context
    )
    {
        _context = context;
    }

    public async Task<List<SceneAction>> FindSceneActions(string sceneId)
    {
        var actions = await _context.SceneActions
            .Where(action =>
                action.SceneId.ToString() == sceneId)
            .Include(action => action.Device)
            .ToListAsync();
        return actions;
    }

    public async Task<SceneAction?> FindSceneActionById(string sceneId, string actionId)
    {
        var action = await _context
            .SceneActions
            .FirstOrDefaultAsync(
                action => action.Id.ToString() == actionId
                && action.SceneId.ToString() == sceneId
            );
        return action;
    }

    public async Task<SceneAction?> FindSceneActionByDeviceId(string sceneId, string deviceId)
    {
        var action = await _context.SceneActions
            .FirstOrDefaultAsync(action => action.DeviceId.ToString() == deviceId && action.SceneId.ToString() == sceneId);
        return action;
    }

    // Find all the smart devices which are not included in the scene
    public async Task<List<SmartDevice>> FindAvailableSmartDevices(SmartHome home, Scene scene)
    {
        var devicesIncludeInSceneQuery = from d2 in _context.SmartDevices
                                         where !(
                                            from d in _context.SmartDevices
                                            join a in _context.SceneActions on d.Id equals a.DeviceId
                                            where d.HomeId == home.Id && a.SceneId == scene.Id
                                            select d.Id
                                        ).Contains(d2.Id) && d2.HomeId == home.Id
                                         select d2;
        return await devicesIncludeInSceneQuery.ToListAsync();
    }

    public async Task DeleteSceneAction(Scene scene, string actionId)
    {
        var action = await FindSceneActionById(scene.Id.ToString(), actionId);
        if (action == null) throw new ModelNotFoundException();

        scene.Actions.Remove(action);
        _context.SceneActions.Remove(action);
        await _context.SaveChangesAsync();
    }
}