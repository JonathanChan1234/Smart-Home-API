using smart_home_server.Scenes.Models;

namespace smart_home_server.Scenes.Services;

public interface ISceneService
{
    Task AddActionToScene(string homeId, string sceneId, SceneAction action);
    Task<Scene> CreateScene(string homeId, string name);
    Task DeleteScene(string homeId, string sceneId);
    Task<Scene> EditSceneName(string homeId, string sceneId, string name);
    Task<Scene?> FindSceneById(string homeId, string sceneId);
    Task<Scene?> FindSceneByName(string homeId, string name);
    Task<List<Scene>> GetHomeScenes(string homeId);
    Task RemoveActionFromScene(string homeId, string sceneId, SceneAction action);
}
