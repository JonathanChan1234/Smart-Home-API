using smart_home_server.Auth.Models;
using smart_home_server.Auth.ResourceModels;

namespace smart_home_server.Auth.Services;

public interface IAuthService
{
    public Task<ApplicationUser?> GetUserById(string id);
    public Task<ApplicationUser?> GetUserByName(string name);
    public Task<TokenResponse> Register(RegisterRequest user);
    public Task<TokenResponse> Login(AuthenticationRequest user);
    public Task<TokenResponse> RefreshToken(string accessToken, string refreshToken);
    public Task Logout(string jti, DateTime expireOn, string refreshToken);
}