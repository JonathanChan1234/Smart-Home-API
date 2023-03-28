using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace smart_home_server.Devices.Models;

[Table("Lights")]
public class Light : Device
{
    public Light(string name, Guid roomId) : base(name, roomId)
    {
    }

    [Required]
    public bool Dimmable { get; set; }
    [Required]
    public int Level { get; set; }
}