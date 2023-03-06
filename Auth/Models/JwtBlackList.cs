using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace smart_home_server.Auth.Models;

[Table("JwtBlackList")]
public class JwtBlackList
{
    [Key]
    public string Jti { get; set; } = null!;
    [Required]
    public DateTime ExpireOn { get; set; }
}