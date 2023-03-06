using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using smart_home_server.Auth.Exceptions;
using smart_home_server.Auth.Models;
using smart_home_server.Auth.Services;
using smart_home_server.Home.Models;
using smart_home_server.Home.ResourceModels;
using smart_home_server.Home.Services;

[ApiController]
[Route("api/v1/[controller]")]
public class HomeController : ControllerBase
{
    private readonly IHomeService _homeService;
    private readonly IAuthService _authService;

    public HomeController(IHomeService homeService, IAuthService authService)
    {
        _homeService = homeService;
        _authService = authService;
    }

    private async Task<ApplicationUser> GetAuthUser()
    {
        var user = await _authService.GetUserById(User.FindFirst(JwtService.USER_ID)?.Value ?? "");
        if (user == null) throw new BadAuthenticationException();
        return user;
    }

    [Authorize]
    [HttpGet]
    public async Task<ActionResult<List<SmartHome>>> GetOwnerHome([FromQuery] SearchOptionsQuery options)
    {
        var home = await _homeService.GetOwnerHome((await GetAuthUser()), options);
        return home;
    }

    [Authorize]
    [HttpGet("{id}/installers")]
    public async Task<ActionResult<List<ApplicationUser>>> GetHomeInstallers(string id)
    {
        var installers = await _homeService.GetHomeInstallers((await GetAuthUser()), id);
        return installers;
    }

    [Authorize]
    [HttpGet("{id}/users")]
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
    public async Task<IActionResult> PostInstaller(string id, [FromBody] PasswordDto dto)
    {
        await _homeService.AddInstallerToHome((await GetAuthUser()), id, dto.Password);
        return CreatedAtAction(nameof(PostInstaller), new { id = id });
    }

    [Authorize]
    [HttpPost("{id}/users")]
    public async Task<IActionResult> PostUser(string id, [FromBody] PasswordDto dto)
    {
        await _homeService.AddUserToHome((await GetAuthUser()), id, dto.Password);
        return CreatedAtAction(nameof(PostInstaller), new { id = id });
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateHome(string id, [FromBody] UpdateHomeDto dto)
    {
        await _homeService.UpdateHome((await GetAuthUser()), id, dto);
        return NoContent();
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteHome(string id)
    {
        await _homeService.DeleteHome((await GetAuthUser()), id);
        return NoContent();
    }
}