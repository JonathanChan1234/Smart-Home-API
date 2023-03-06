using System.ComponentModel.DataAnnotations;

namespace smart_home_server.Auth.ResourceModels;

public class RegisterRequest
{
    [Required]
    public string UserName { get; set; }

    [Required]
    public string Password { get; set; }

    [Required]
    public string Email { get; set; }

    public RegisterRequest(string username, string email, string password)
    {
        UserName = username;
        Password = password;
        Email = email;
    }
}