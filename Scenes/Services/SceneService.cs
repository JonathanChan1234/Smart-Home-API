using Microsoft.EntityFrameworkCore;
using smart_home_server.Db;
using smart_home_server.Scenes.Models;
using smart_home_server.Exceptions;
using smart_home_server.Home.Services;

namespace smart_home_server.Scenes.Services;

public class SceneService : ISceneService
{
    private readonly AppDbContext _context;
    private readonly IHomeService _homeService;

    public SceneService(AppDbContext context, IHomeService homeService)
    {
        _context = context;
        _homeService = homeService;
    }

    public async Task<Scene?> FindSceneById(string homeId, string sceneId)
    {
        var scene = await _context.Scenes
            .Where(scene => scene.Id.ToString() == sceneId && scene.HomeId.ToString() == homeId)
            .FirstOrDefaultAsync();
        return scene;
    }

    public async Task<Scene?> FindSceneByName(string homeId, string name)
    {
        var scene = await _context.Scenes
            .Where(scene => scene.Name == name && scene.HomeId.ToString() == homeId)
            .FirstOrDefaultAsync();
        return scene;
    }

    public async Task<List<Scene>> GetHomeScenes(string homeId)
    {
        var scenes = await _context.Scenes.
            Where(scene => scene.HomeId.ToString() == homeId)
            .ToListAsync();
        return scenes;
    }

    public async Task<Scene> CreateScene(string homeId, string name)
    {
        var sceneWithSameName = await FindSceneByName(homeId, name);
        if (sceneWithSameName != null) throw new BadRequestException($"Scene with name {name} already exists");

        var home = await _homeService.GetHomeById(homeId, null);
        if (home == null) throw new BadRequestException($"home (id: {homeId}) does not exist");

        var scene = new Scene()
        {
            Home = home,
            Name = name
        };

        _context.Scenes.Add(scene);
        await _context.SaveChangesAsync();
        return scene;
    }

    public async Task AddActionToScene(string homeId, string sceneId, SceneAction action)
    {
        var scene = await FindSceneById(homeId, sceneId);
        if (scene == null) throw new BadRequestException("Scene (id: {sceneId}) does not exist");

        scene.Actions.Add(action);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveActionFromScene(string homeId, string sceneId, SceneAction action)
    {
        var scene = await FindSceneById(homeId, sceneId);
        if (scene == null) throw new BadRequestException("Scene (id: {sceneId}) does not exist");

        scene.Actions.Remove(action);
        await _context.SaveChangesAsync();
    }

    public async Task<Scene> EditSceneName(string homeId, string sceneId, string name)
    {
        var scene = await FindSceneById(homeId, sceneId);
        if (scene == null) throw new BadRequestException("Scene (id: {sceneId}) does not exist");

        var sceneWithSameName = await FindSceneByName(homeId, name);
        if (sceneWithSameName != null && sceneWithSameName.Id.ToString() != sceneId)
            throw new BadRequestException($"Scene with name {name} already exists");

        var home = await _homeService.GetHomeById(homeId, null);
        if (home == null) throw new BadRequestException($"home (id: {homeId}) does not exist");

        scene.Name = name;
        await _context.SaveChangesAsync();
        return scene;
    }

    public async Task DeleteScene(string homeId, string sceneId)
    {
        var scene = await FindSceneById(homeId, sceneId);
        if (scene == null) throw new BadRequestException("Scene (id: {sceneId}) does not exist");

        _context.Scenes.Remove(scene);
        await _context.SaveChangesAsync();
    }
}