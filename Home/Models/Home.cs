using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using smart_home_server.Auth.Models;

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

    [ForeignKey("OwnerId")]
    [JsonIgnore]
    public ApplicationUser Owner { get; set; } = null!;

    [Required]
    public String InstallerPassword { get; set; } = null!;

    [Required]
    public String UserPassword { get; set; } = null!;

    public String Description { get; set; } = null!;

    public IList<Floor> Floors { get; set; } = null!;

    public IList<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();

    public IList<ApplicationUser> Installers { get; set; } = new List<ApplicationUser>();
}