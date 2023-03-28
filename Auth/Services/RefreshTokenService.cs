using smart_home_server.Auth.Models;
using smart_home_server.Db;
using smart_home_server.Exceptions;
using smart_home_server.Utils;

namespace smart_home_server.Auth.Services;

public interface IRefreshTokenService
{
    /// <summary>
    /// Method <c>CreateRefreshToken</c> creates a new refresh token with user and jti
    /// </summary>
    public Task<RefreshToken> CreateRefreshToken(string userId, string jti);

    /// <summary>
    /// Method <c>UpdateRefreshToken</c> check if the refresh token is valid and make it to be used if valid
    /// </summary>
    public Task<RefreshToken> UpdateRefreshToken(string refreshToken, string jti);

    /// <summary>
    /// Method <c>RevokeRefreskToken</c> revoke the refresh token which will not be used anymore 
    /// </summary>
    public Task RevokeRefreshToken(string refreshToken, string jti);
}

public class RefreshTokenService : IRefreshTokenService
{
    private readonly AppDbContext _context;
    public RefreshTokenService(AppDbContext context)
    {
        _context = context;
    }

    public async Task RevokeRefreshToken(string refreshToken, string jti)
    {
        var existingRefreshToken = _context.RefreshTokens.FirstOrDefault(r => r.Token == refreshToken);
        if (existingRefreshToken == null) throw new BadRequestException("Invalid refresh token");

        if (existingRefreshToken.JwtId != jti) throw new BadRequestException("Jti not matched");

        existingRefreshToken.IsRevoked = true;
        await _context.SaveChangesAsync();
    }

    public async Task<RefreshToken> UpdateRefreshToken(string refreshToken, string jti)
    {
        // 4. Check if the refreshToken exists
        var existingRefreshToken = _context.RefreshTokens.FirstOrDefault(r => r.Token == refreshToken);
        if (existingRefreshToken == null) throw new BadRequestException("Invalid refresh token");

        // 5. Check if the refreshToken expires or not
        if (existingRefreshToken.ExpiryDate < DateTime.UtcNow) throw new BadRequestException("Refresh token expired");

        // 6. Check if the refreshToken matches the jti of accessToken
        if (existingRefreshToken.JwtId != jti) throw new BadRequestException("Jti not matched");

        // 7. Check if the refreshToken is used or not
        if (existingRefreshToken.IsUsed) throw new BadRequestException("Refresh Token used");

        // 8. Check if the refreshToken is revoked or not
        if (existingRefreshToken.IsRevoked) throw new BadRequestException("Refresh Token revoked");

        existingRefreshToken.IsUsed = true;
        await _context.SaveChangesAsync();
        return existingRefreshToken;
    }

    public async Task<RefreshToken> CreateRefreshToken(String userId, string jti)
    {
        var refreshToken = new RefreshToken
        {
            JwtId = jti,
            IsUsed = false,
            IsRevoked = false,
            UserId = userId,
            AddedDate = DateTime.UtcNow,
            ExpiryDate = DateTime.UtcNow.AddMonths(6),
            Token = Guid.NewGuid() + StringUtils.RandomString(25)
        };
        await _context.RefreshTokens.AddAsync(refreshToken);
        await _context.SaveChangesAsync();
        return refreshToken;
    }
}