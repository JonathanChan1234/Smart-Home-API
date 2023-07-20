using Microsoft.AspNetCore.Authorization;
using smart_home_server.Auth.Services;
using smart_home_server.Home.Models;

namespace smart_home_server.Home.Authorization;

public class HomeAuthorizationHandler : AuthorizationHandler<HomeOperationRequirement, SmartHome>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, HomeOperationRequirement requirement, SmartHome home)
    {
        var userId = context.User.FindFirst(JwtService.USER_ID)?.Value;
        if (userId == null)
        {
            context.Fail(new AuthorizationFailureReason(this, "user id not found"));
            return Task.CompletedTask;
        }

        var installer = home.Installers.FirstOrDefault(i => i.Id == userId);
        var user = home.Users.FirstOrDefault(u => u.Id == userId);
        var isOwner = home.OwnerId == userId;

        if (requirement.Name == HomeOperation.User.Name)
        {
            if (user == null && !isOwner)
            {
                context.Fail(new AuthorizationFailureReason(this, "home user not found"));
                return Task.CompletedTask;
            }
        }
        if (requirement.Name == HomeOperation.Installer.Name)
        {
            if (installer == null && !isOwner)
            {
                context.Fail(new AuthorizationFailureReason(this, "home installer not found"));
                return Task.CompletedTask;
            }
        }
        if (requirement.Name == HomeOperation.Owner.Name)
        {
            if (!isOwner)
            {
                context.Fail(new AuthorizationFailureReason(this, "home owner not found"));
                return Task.CompletedTask;
            }
        }
        if (requirement.Name == HomeOperation.All.Name)
        {
            if (installer == null && user == null && !isOwner)
            {
                context.Fail(new AuthorizationFailureReason(this, "home owner not found"));
                return Task.CompletedTask;
            }
        }
        context.Succeed(requirement);
        return Task.CompletedTask;
    }
}

public class HomeOperationRequirement : IAuthorizationRequirement
{
    public string Name = nameof(HomeOperation.All);
}

public static class HomeOperation
{
    public static HomeOperationRequirement User = new HomeOperationRequirement { Name = nameof(User) };
    public static HomeOperationRequirement Owner = new HomeOperationRequirement { Name = nameof(Owner) };
    public static HomeOperationRequirement Installer = new HomeOperationRequirement { Name = nameof(Installer) };
    public static HomeOperationRequirement All = new HomeOperationRequirement { Name = nameof(All) };
}