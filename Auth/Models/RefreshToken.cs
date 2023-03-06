using System.ComponentModel.DataAnnotations.Schema;

namespace smart_home_server.Auth.Models;

[Table("RefreshToken")]
public class RefreshToken
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public string Id { get; set; } = null!;
    public string Token { get; set; } = null!;
    public string UserId { get; set; } = null!;
    public string JwtId { get; set; } = null!;
    public bool IsUsed { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime AddedDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    [ForeignKey(nameof(UserId))]
    public ApplicationUser User { get; set; } = null!;
}