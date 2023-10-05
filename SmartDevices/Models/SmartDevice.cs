using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using smart_home_server.Home.Models;
using smart_home_server.Scenes.Models;

namespace smart_home_server.SmartDevices.Models;

public enum MainCategory
{
    [Description("light")]
    Light,
    [Description("shade")]
    Shade,
    [Description("ac")]
    AirConditioner
}

public enum SubCategory
{

    [Description("dimmer")]
    Dimmer,
    [Description("light-switch")]
    LightSwitch,
    [Description("motor-shade")]
    MotorShade,
    [Description("roller-shade")]
    RollerShade,
    [Description("air-conditioner")]
    AirConditioner
}

[Table("SmartDevices")]
public class SmartDevice
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = null!;

    public Guid RoomId { get; set; }

    [ForeignKey("RoomId")]
    [JsonIgnore]
    public Room Room { get; set; } = null!;

    public Guid HomeId { get; set; }

    [ForeignKey("HomeId")]
    [JsonIgnore]
    public SmartHome Home { get; set; } = null!;

    public MainCategory MainCategory { get; set; }

    public SubCategory SubCategory { get; set; }

    public Dictionary<string, object?> Properties { get; set; } = new();

    public Dictionary<string, object?> Capabilities { get; set; } = new();

    public bool OnlineStatus { get; set; }

    public DateTime StatusLastUpdatedAt { get; set; }

    [JsonIgnore]
    public IList<SceneAction> Actions { get; set; } = new List<SceneAction>();
}