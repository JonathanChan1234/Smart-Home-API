using System.ComponentModel.DataAnnotations;

namespace smart_home_server.Home.ResourceModels;

// Plain object for creating floor and name
public class NameDto
{
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = null!;
}