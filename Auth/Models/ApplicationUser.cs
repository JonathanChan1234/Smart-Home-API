using Microsoft.AspNetCore.Identity;

using smart_home_server.Home.Models;

namespace smart_home_server.Auth.Models;

public class ApplicationUser : IdentityUser
{
    public IList<SmartHome> OwnerHome { get; set; } = null!;
    public IList<SmartHome> UserHome { get; set; } = null!;
    public IList<SmartHome> InstallerHome { get; set; } = null!;
}