
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace smart_home_server.Devices.Models;

[Table("Shades")]
public class Shade : Device
{
    public Shade(string name, Guid roomId) : base(name, roomId) { }
    [Required]
    public bool HasLevel { get; set; }
    [Required]
    public int Level { get; set; }
}