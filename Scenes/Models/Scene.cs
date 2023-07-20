using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using smart_home_server.Home.Models;

namespace smart_home_server.Scenes.Models;

[Table("scenes")]
public class Scene
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public Guid HomeId { get; set; }

    [ForeignKey("HomeId")]
    [JsonIgnore]
    public SmartHome Home { get; set; } = null!;

    public IList<SceneAction> Actions { get; set; } = new List<SceneAction>();
}