using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using smart_home_server.Auth.Models;
using smart_home_server.Home.Models;

namespace smart_home_server.Mqtt.Client.Models;

[Table("MqttClients")]
public class MqttClient
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public int ClientId { get; set; }

    public string UserId { get; set; } = null!;

    [JsonIgnore]
    [ForeignKey("UserId")]
    public ApplicationUser User { get; set; } = null!;

    public Guid HomeId { get; set; }

    [JsonIgnore]
    [ForeignKey("HomeId")]
    public SmartHome Home { get; set; } = null!;

    public bool Revoked { get; set; }

    public DateTime CreatedAt { get; set; }
}