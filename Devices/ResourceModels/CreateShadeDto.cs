
using Microsoft.AspNetCore.Mvc;

namespace smart_home_server.Devices.ResourceModels;
public class CreateShadeDto
{
    [FromBody]
    public String Name { get; set; } = null!;
    [FromBody]
    public bool HasLevel { get; set; }
}