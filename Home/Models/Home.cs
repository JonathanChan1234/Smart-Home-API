using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using smart_home_server.Auth.Models;
using smart_home_server.Processors.Models;
using smart_home_server.Scenes.Models;
using smart_home_server.SmartDevices.Models;

namespace smart_home_server.Home.Models;

[Table("SmartHome")]
[PrimaryKey(nameof(Id))]
public class SmartHome
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = null!;

    public string OwnerId { get; set; } = null!;

    public Guid? ProcessorId { get; set; }

    [ForeignKey("ProcessorId")]
    public Processor? Processor { get; set; } = null!;

    [ForeignKey("OwnerId")]
    [JsonIgnore]
    public ApplicationUser Owner { get; set; } = null!;

    [JsonIgnore]
    [Required]
    public String InstallerPassword { get; set; } = null!;

    [JsonIgnore]
    [Required]
    public String UserPassword { get; set; } = null!;

    public String Description { get; set; } = null!;

    [JsonIgnore]
    public IList<Floor> Floors { get; set; } = null!;

    [JsonIgnore]
    public IList<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();

    [JsonIgnore]
    public IList<ApplicationUser> Installers { get; set; } = new List<ApplicationUser>();

    [JsonIgnore]
    public IList<Scene> Scenes { get; set; } = new List<Scene>();

    [JsonIgnore]
    public IList<SmartDevice> SmartDevices { get; set; } = new List<SmartDevice>();
}