using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using smart_home_server.Auth.Exceptions;
using smart_home_server.Auth.Models;
using smart_home_server.Auth.Services;
using smart_home_server.Exceptions;
using smart_home_server.Home.Authorization;
using smart_home_server.Home.Models;
using smart_home_server.Home.Services;
using smart_home_server.Processors.Models;
using smart_home_server.Processors.Service;

namespace smart_home_server.Processors.Controller;

[ApiController]
[Route("api/v1/home/{homeId}/[controller]")]
public class ProcessorController : ControllerBase
{
    private readonly IProcessorService _processorService;
    private readonly IHomeService _homeService;
    private readonly IAuthorizationService _authorizationService;
    private readonly IAuthService _authService;

    public ProcessorController(
        IHomeService homeService,
        IProcessorService processorServices,
        IAuthorizationService authorizationService,
        IAuthService authService)
    {
        _homeService = homeService;
        _processorService = processorServices;
        _authorizationService = authorizationService;
        _authService = authService;
    }

    private async Task<SmartHome> GetHomeByParams(string homeId)
    {
        var home = await _homeService.GetHomeById(homeId, null);
        if (home == null)
            throw new ModelNotFoundException($"Cannot find home {homeId}");

        if (!(await _authorizationService.AuthorizeAsync(User, home, HomeOperation.Installer)).Succeeded)
            throw new ForbiddenException($"No permisson for home {homeId}");
        return home;
    }

    private async Task<ApplicationUser> GetAuthUser()
    {
        var userId = HttpContext.User.FindFirst(JwtService.USER_ID)?.Value;
        if (userId == null) throw new BadAuthenticationException("Empty User ID");
        var user = await _authService.GetUserById(userId);
        if (user == null) throw new BadAuthenticationException("Non existing user");
        return user;
    }

    [HttpGet]
    public async Task<ActionResult<Processor>> GetHomeProcessor(string homeId)
    {
        var processor = await _processorService.GetProcessorByHomeId((await GetHomeByParams(homeId)).Id.ToString());
        if (processor == null) throw new ModelNotFoundException($"Processor (Home ID: {homeId}) does not exist");
        return processor;
    }

    [HttpPost]
    public async Task<ActionResult<Processor>> CreateHomeProcessor(string homeId)
    {
        var processor = await _processorService.CreateNewProcessor(await GetAuthUser(), await GetHomeByParams(homeId));
        return CreatedAtAction(nameof(CreateHomeProcessor), new { id = processor.Id }, processor);
    }

    [HttpDelete]
    public async Task<ActionResult> DeleteHomeProcessorStatus(string homeId)
    {
        await _processorService.DeleteProcessorAtHome((await GetHomeByParams(homeId)).Id.ToString());
        return NoContent();
    }
}