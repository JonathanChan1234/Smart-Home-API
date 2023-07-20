using System.Text.Json.Serialization;

namespace smart_home_server.Scenes.Models;

public enum ShadeActionType
{
    raise,
    lower,
    na,
}

public class ShadeAction
{
    public ShadeActionType? ActionType { get; set; }
    public int? Level { get; set; }
}