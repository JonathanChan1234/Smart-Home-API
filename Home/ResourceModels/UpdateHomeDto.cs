using Microsoft.AspNetCore.Mvc;

namespace smart_home_server.Home.ResourceModels;

public class UpdateHomeDto
{

    [FromBody]
    public string? Name { get; set; } = null!;
    [FromBody]
    public string? Description { get; set; } = null!;
    [FromBody]
    public string? InstallerPassword { get; set; } = null!;
    [FromBody]
    public string? UserPassword { get; set; } = null!;
}