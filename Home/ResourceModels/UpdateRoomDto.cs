using Microsoft.AspNetCore.Mvc;

namespace smart_home_server.Home.ResourceModels;

public class UpdateRoomDto
{
    [FromBody]
    public string? Name { get; set; }
    [FromBody]
    public bool? IsFavorite { get; set; }
}