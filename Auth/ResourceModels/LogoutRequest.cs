namespace smart_home_server.Auth.ResourceModels;

public class LogoutRequest
{
    public string RefreshToken { get; set; } = null!;
}