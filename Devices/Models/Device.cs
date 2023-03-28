using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using smart_home_server.Home.Models;

namespace smart_home_server.Devices.Models;

[Table("Devices")]
public class Device
{
    protected Device(string name, Guid roomId)
    {
        Name = name;
        RoomId = roomId;
    }

    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(100)]
    public String Name { get; set; } = null!;

    public Guid RoomId { get; set; }

    [ForeignKey("RoomId")]
    [JsonIgnore]
    public Room Room { get; set; } = null!;
    public DateTime StatusLastUpdatedAt { get; set; }
}