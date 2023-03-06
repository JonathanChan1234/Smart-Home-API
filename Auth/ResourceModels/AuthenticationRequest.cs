namespace smart_home_server.Auth.ResourceModels;

public class AuthenticationRequest
{
    public String UserName { get; set; }
    public String Password { get; set; }
    public AuthenticationRequest(String username, String password)
    {
        UserName = username;
        Password = password;
    }
}