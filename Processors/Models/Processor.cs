using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using smart_home_server.Home.Models;
using smart_home_server.Mqtt.Client.Models;

namespace smart_home_server.Processors.Models;

[Table("Processors")]
public class Processor
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    [JsonIgnore]
    public SmartHome Home { get; set; } = null!;

    public int MqttClientId { get; set; }

    [ForeignKey("MqttClientId")]
    public MqttClient MqttClient { get; set; } = null!;

    public bool OnlineStatus { get; set; }

    [JsonIgnore]
    public string Password { get; set; } = null!;

    public DateTime AddedAt { get; set; }
}