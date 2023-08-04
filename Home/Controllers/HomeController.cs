using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using smart_home_server.Auth.Exceptions;
using smart_home_server.Auth.Models;
using smart_home_server.Auth.Services;
using smart_home_server.Exceptions;
using smart_home_server.Home.Authorization;
using smart_home_server.Home.Models;
using smart_home_server.Home.ResourceModels;
using smart_home_server.Home.Services;

[ApiController]
[Route("api/v1/[controller]")]
public class HomeController : ControllerBase
{
    private readonly IHomeService _homeService;
    private readonly IAuthService _authService;
    private readonly IAuthorizationService _authorizationService;

    public HomeController(
        IHomeService homeService,
        IAuthService authService,
        IAuthorizationService authorizationService)
    {
        _homeService = homeService;
        _authService = authService;
        _authorizationService = authorizationService;
    }

    private async Task<SmartHome> GetHomeByParams(string homeId, HomeOperationRequirement right)
    {
        var home = await _homeService.GetHomeById(homeId, null);
        if (home == null)
            throw new ModelNotFoundException($"Cannot find home {homeId}");

        if (!(await _authorizationService.AuthorizeAsync(User, home, right)).Succeeded)
            throw new ForbiddenException($"No permisson for home {homeId}");
        return home;
    }

    private async Task<ApplicationUser> GetAuthUser()
    {
        var user = await _authService.GetUserById(User.FindFirst(JwtService.USER_ID)?.Value ?? "");
        if (user == null) throw new BadAuthenticationException();
        return user;
    }

    [Authorize]
    [HttpGet("owner")]
    public async Task<ActionResult<List<SmartHome>>> GetOwnerHome([FromQuery] SearchOptionsQuery options)
    {
        var home = await _homeService.GetOwnerHome((await GetAuthUser()), options);
        return home;
    }

    [Authorize]
    [HttpGet("user")]
    public async Task<ActionResult<List<SmartHome>>> GetUserHome([FromQuery] SearchOptionsQuery options)
    {
        var home = await _homeService.GetUserHome((await GetAuthUser()), options);
        return home;
    }

    [Authorize]
    [HttpGet("installer")]
    public async Task<ActionResult<List<SmartHome>>> GetInstallerHome([FromQuery] SearchOptionsQuery options)
    {
        var home = await _homeService.GetInstallerHome((await GetAuthUser()), options);
        return home;
    }

    [Authorize]
    [HttpGet("{id}/installer")]
    public async Task<ActionResult<List<ApplicationUser>>> GetHomeInstallers(string id)
    {
        var installers = await _homeService.GetHomeInstallers((await GetAuthUser()), id);
        return installers;
    }

    [Authorize]
    [HttpGet("{id}/user")]
    public async Task<ActionResult<List<ApplicationUser>>> GetHomeUsers(string id)
    {
        var users = await _homeService.GetHomeUsers((await GetAuthUser()), id);
        return users;
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> PostHome([FromBody] CreateHomeDto dto)
    {
        var model = await _homeService.CreateHome((await GetAuthUser()), dto);
        return CreatedAtAction(nameof(PostHome), new { id = model.Id }, model);
    }

    [Authorize]
    [HttpPost("{id}/installer")]
    public async Task<ActionResult<SmartHome>> PostInstaller(string id, [FromBody] PasswordDto dto)
    {
        var home = await _homeService.AddInstallerToHome((await GetAuthUser()), id, dto.Password);
        return CreatedAtAction(nameof(PostInstaller), new { id = id }, home);
    }

    [Authorize]
    [HttpPost("{id}/user")]
    public async Task<ActionResult<SmartHome>> PostUser(string id, [FromBody] PasswordDto dto)
    {
        var home = await _homeService.AddUserToHome((await GetAuthUser()), id, dto.Password);
        return CreatedAtAction(nameof(PostUser), new
        {
            id = id
        }, home);
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateHome(string id, [FromBody] UpdateHomeDto dto)
    {
        await _homeService.UpdateHome(await GetHomeByParams(id, HomeOperation.Owner), dto);
        return NoContent();
    }

    [Authorize]
    [HttpDelete("{id}/user")]
    public async Task<IActionResult> DeleteHomeUser(string id)
    {
        await _homeService.RemoveUserFromHome(await GetAuthUser(), id);
        return NoContent();
    }

    [Authorize]
    [HttpDelete("{id}/installer")]
    public async Task<IActionResult> DeleteHomeInstaller(string id)
    {
        await _homeService.RemoveInstallerFromHome(await GetAuthUser(), id);
        return NoContent();
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteHome(string id)
    {
        await _homeService.DeleteHome(await GetHomeByParams(id, HomeOperation.Owner));
        return NoContent();
    }
}