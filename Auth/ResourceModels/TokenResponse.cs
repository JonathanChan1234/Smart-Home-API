namespace smart_home_server.Auth.ResourceModels;

public class TokenResponse
{
    public String AccessToken { get; set; } = null!;
    public String RefreshToken { get; set; } = null!;

    public TokenResponse(string accessToken, string refreshToken)
    {
        AccessToken = accessToken;
        RefreshToken = refreshToken;
    }
}