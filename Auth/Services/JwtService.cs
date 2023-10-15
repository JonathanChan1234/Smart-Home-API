using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.IdentityModel.Tokens;
using smart_home_server.Auth.Exceptions;
using smart_home_server.Auth.Models;
using smart_home_server.Db;
using smart_home_server.Exceptions;
using smart_home_server.Utils;

namespace smart_home_server.Auth.Services;

public enum Role { master, installer, user }

public class JwtNotExpiredException : Exception
{
    public JwtNotExpiredException()
    {
    }

    public JwtNotExpiredException(string? message) : base(message)
    {
    }
}

public class JwtService : IJwtService
{
    public static string USER_ID = "userId";
    private const int EXPIRATION_TIMEOUT = 30;
    private readonly IConfiguration _configuration;
    private readonly AppDbContext _context;
    private readonly TokenValidationParameters _validationParameters;

    public JwtService(IConfiguration configuration, AppDbContext context, TokenValidationParameters validationParameters)
    {
        _configuration = configuration;
        _context = context;
        _validationParameters = validationParameters;
    }

    public async Task<bool> ValidateJwt(string accessToken, string userId)
    {
        var jwtTokenHandler = new JwtSecurityTokenHandler();

        // Decode the JWT and retrieve the claim
        var claim = jwtTokenHandler.ValidateToken(accessToken, _validationParameters, out SecurityToken securityToken);
        if (claim == null) return false;

        // Check if the jwt is in the black list
        var revoked = await CheckIfJwtInBlackList(securityToken.Id);
        if (revoked) return false;

        // Check if the jwt belongs to the user
        return claim.Claims.FirstOrDefault(c => c.Type == JwtService.USER_ID)?.Value == userId;
    }

    /// Check if the jwt is valid for renewal using refresh token
    public JwtSecurityToken? CheckIfJwtIsValidForRenew(string accessToken)
    {
        var jwtTokenHandler = new JwtSecurityTokenHandler();
        // 1. Check if the access token is valid jwt
        var jwtSecurityToken = jwtTokenHandler.ReadJwtToken(accessToken);
        if (!jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            throw new BadRequestException("Wrong security algorithm");

        // 3. Check if the accessToken expires or not
        var exp = jwtSecurityToken?.Payload.Exp;
        if (exp == null) throw new BadRequestException("Missing expire time");
        if (DateUtils.UnixTimeStampToDateTime((double)exp) > DateTime.UtcNow.AddHours(8))
            throw new JwtNotExpiredException("Access token not expired");

        return jwtSecurityToken;
    }

    public JwtSecurityToken CreateAccessToken(String userId, String username)
    {
        var expiration = DateTime.UtcNow.AddMinutes(EXPIRATION_TIMEOUT);
        var token = CreateJwtToken(
            CreateClaims(userId, username),
            CreateSigningCredientials(),
            expiration
        );
        return token;
    }

    public async Task AddJwtToBlackList(string jti, DateTime expireOn)
    {
        var existingJwt = await _context.JwtBlackList.FindAsync(jti);
        // the jwt already is in the database
        if (existingJwt != null) return;
        var jwt = new JwtBlackList
        {
            Jti = jti,
            ExpireOn = expireOn,
        };
        _context.JwtBlackList.Add(jwt);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> CheckIfJwtInBlackList(string jti)
    {
        var jwt = await _context.JwtBlackList.FindAsync(jti);
        return jwt != null;
    }

    private JwtSecurityToken CreateJwtToken(Claim[] claims, SigningCredentials credentials, DateTime expiration)
    {
        return new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                claims: claims,
                expires: expiration,
                signingCredentials: credentials
            );

    }
    private Claim[] CreateClaims(String userId, String username)
    {
        var subject = _configuration["Jwt:Subject"];
        if (subject == null)
            throw new JwtMissingParameterException("Parameter Missing");
        return new[] {
            new Claim(JwtRegisteredClaimNames.Sub, subject),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTime.Now.ToString()),
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Name, username),
            new Claim(USER_ID, userId),
        };
    }

    private SigningCredentials CreateSigningCredientials()
    {
        var key = _configuration["Jwt:Key"];
        if (key == null) throw new JwtMissingParameterException("Missing configuraiton parameter Jwt:Key");
        return new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256
            );
    }
}