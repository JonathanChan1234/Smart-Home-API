using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Server.IIS.Core;
using smart_home_server.Auth.Exceptions;
using smart_home_server.Auth.Models;
using smart_home_server.Auth.ResourceModels;
using smart_home_server.Auth.Services;
using smart_home_server.Exceptions;

public class AuthService : IAuthService
{
    private readonly IJwtService _jwtService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly UserManager<ApplicationUser> _userManager;

    public AuthService(
        IJwtService jwtService,
        IRefreshTokenService refreshTokenService,
        UserManager<ApplicationUser> userManager)
    {
        _jwtService = jwtService;
        _refreshTokenService = refreshTokenService;
        _userManager = userManager;
    }

    private async Task<TokenResponse> CreateToken(ApplicationUser user)
    {
        var jwt = _jwtService.CreateAccessToken(user.Id, user.UserName ?? "");
        var refreshToken = await _refreshTokenService.CreateRefreshToken(user.Id, jwt.Id);
        return new TokenResponse(new JwtSecurityTokenHandler().WriteToken(jwt), refreshToken.Token);
    }

    public Task<ApplicationUser?> GetUserById(string id)
    {
        return _userManager.FindByIdAsync(id);
    }

    public Task<ApplicationUser?> GetUserByName(string name)
    {
        return _userManager.FindByNameAsync(name);
    }

    public async Task<TokenResponse> Register(RegisterRequest user)
    {
        var result = await _userManager.CreateAsync(new ApplicationUser()
        {
            UserName = user.UserName,
            Email = user.Email
        }, user.Password);
        if (!result.Succeeded) throw new Exception(String.Join(",", result.Errors.Select(error => error.Description)));

        var identityUser = await _userManager.FindByNameAsync(user.UserName)
            ?? throw new UserNotFoundException();
        return await CreateToken(identityUser);
    }

    public async Task<TokenResponse> Login(AuthenticationRequest user)
    {
        var identityUser = await _userManager.FindByNameAsync(user.UserName)
            ?? throw new UserNotFoundException();
        if (!await _userManager.CheckPasswordAsync(identityUser, user.Password)) throw new BadAuthenticationException();

        return await CreateToken(identityUser);
    }

    public async Task<TokenResponse> RefreshToken(string accessToken, string refreshToken)
    {
        try
        {
            var jwtSecurityToken = _jwtService.CheckIfJwtIsValidForRenew(accessToken)
                ?? throw new BadRequestException("JWT missing claims");
            var jti = jwtSecurityToken.Id;
            var existingRefreshToken = await _refreshTokenService.UpdateRefreshToken(refreshToken, jti);

            var identityUser = await _userManager.FindByIdAsync(existingRefreshToken.UserId)
                ?? throw new BadRequestException("User not found");
            return await CreateToken(identityUser);
        }
        catch (JwtNotExpiredException)
        {
            // if the token is not expired, return the original token to the user
            return new TokenResponse(accessToken, refreshToken);
        }
    }

    public async Task Logout(string jti, DateTime expireOn, string refreshToken)
    {
        await _refreshTokenService.RevokeRefreshToken(refreshToken, jti);
        await _jwtService.AddJwtToBlackList(jti, expireOn);
    }
}