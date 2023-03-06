using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace smart_home_server.Home.Models;

[Table("Floors")]
[PrimaryKey(nameof(Id))]
public class Floor
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(50)]
    public String Name { get; set; } = null!;

    public Guid HomeId { get; set; }

    [ForeignKey("HomeId")]
    [JsonIgnore]
    public SmartHome Home { get; set; } = null!;

    public IList<Room> Rooms { get; set; } = null!;
}