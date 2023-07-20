
using Microsoft.AspNetCore.Authorization;
using smart_home_server.Auth.Services;
using smart_home_server.Scenes.Models;

namespace smart_home_server.Scenes.Authorization;
public class SceneAuthorizationHandler : AuthorizationHandler<SceneOperationRequirement, Scene>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, SceneOperationRequirement requirement, Scene scene)
    {
        var userId = context.User.FindFirst(JwtService.USER_ID)?.Value;
        if (userId == null)
        {
            context.Fail(new AuthorizationFailureReason(this, "user id not found"));
            return Task.CompletedTask;
        }
        context.Succeed(requirement);
        return Task.CompletedTask;
    }
}

public class SceneOperationRequirement : IAuthorizationRequirement
{
    public SceneOperationRequirement(String homeId)
    {
        HomeId = homeId;
    }
    public String HomeId;
}