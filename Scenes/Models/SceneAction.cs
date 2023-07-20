using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using smart_home_server.SmartDevices.Models;

namespace smart_home_server.Scenes.Models;

[Table("actions")]
public class SceneAction
{

    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    public string Description { get; set; } = null!;

    public Guid DeviceId { get; set; }

    [ForeignKey("DeviceId")]
    public SmartDevice Device { get; set; } = null!;

    public Guid SceneId { get; set; }

    [ForeignKey("SceneId")]
    [JsonIgnore]
    public Scene Scene { get; set; } = null!;

    public Dictionary<string, object?> Action { get; set; } = new();
}