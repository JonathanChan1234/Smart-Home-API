using Microsoft.AspNetCore.Mvc;

namespace smart_home_server.Devices.ResourceModels;
public class UpdateLightDto
{
    [FromBody]
    public String Name { get; set; } = null!;
    [FromBody]
    public bool Dimmable { get; set; }
}