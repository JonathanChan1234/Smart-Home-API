using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using smart_home_server.Auth.Exceptions;
using smart_home_server.Auth.ResourceModels;
using smart_home_server.Auth.Services;
using smart_home_server.Utils;

namespace smart_home_server.Auth.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("Register")]
    public async Task<ActionResult<TokenResponse>> Register(RegisterRequest user)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try
        {
            var result = await _authService.Register(user);
            return CreatedAtAction(nameof(Register), new { username = user.UserName }, result);
        }
        catch (Exception exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpPost("Login")]
    public async Task<ActionResult<TokenResponse>> Login(AuthenticationRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try
        {
            var result = await _authService.Login(request);
            return Ok(result);
        }
        catch (UserNotFoundException)
        {
            return BadRequest(new { message = "Model Not Found" });
        }
        catch (BadAuthenticationException)
        {
            return BadRequest(new { message = "Bad Credentials" });
        }
    }

    [HttpPost("RefreshToken")]
    public async Task<ActionResult<TokenResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        return await _authService.RefreshToken(request.AccessToken, request.RefreshToken);
    }

    [Authorize]
    [HttpPost("Logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
    {
        var jti = User.Claims.FirstOrDefault(c => c.Type == "jti")?.Value;
        var expireOn = User.Claims.FirstOrDefault(c => c.Type == "exp")?.Value;
        if (jti == null || expireOn == null) return BadRequest();

        bool parseSuccess = Double.TryParse(expireOn, out double expireTimeInSeconds);
        if (!parseSuccess) return BadRequest(new { message = "Bad JWT Expire Time" });

        DateTime datetime = DateUtils.UnixTimeStampToDateTime(expireTimeInSeconds);
        await _authService.Logout(jti, datetime, request.RefreshToken);

        return Ok();
    }
}