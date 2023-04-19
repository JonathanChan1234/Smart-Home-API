using System.IdentityModel.Tokens.Jwt;

namespace smart_home_server.Auth.Services;

public interface IJwtService
{
    Task<bool> ValidateJwt(string accessToken, string userId);
    JwtSecurityToken CreateAccessToken(string userId, string username);
    Task AddJwtToBlackList(string jti, DateTime expireOn);
    Task<bool> CheckIfJwtInBlackList(string jti);
    JwtSecurityToken? CheckIfJwtIsValidForRenew(string accessToken);
}
