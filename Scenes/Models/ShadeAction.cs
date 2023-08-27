using System.Text.Json.Serialization;

namespace smart_home_server.Scenes.Models;

public enum ShadeActionType
{
    raise,
    na,
    lower,
}

public class ShadeAction
{
    public ShadeActionType? ActionType { get; set; }
    public int? Level { get; set; }
}