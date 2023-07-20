using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using smart_home_server.SmartDevices.Models;

namespace smart_home_server.Home.Models;

[Table("Rooms")]
[PrimaryKey(nameof(Id))]
public class Room
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(50)]
    public String Name { get; set; } = null!;

    public bool IsFavorite { get; set; }

    public Guid FloorId { get; set; }

    [JsonIgnore]
    public Floor Floor { get; set; } = null!;

    [JsonIgnore]
    public List<SmartDevice> SmartDevices { get; set; } = null!;
}