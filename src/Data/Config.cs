using System.Text.Json.Serialization;
using CounterStrikeSharp.API.Core;

namespace HidePlayers;

public class PluginConfig : BasePluginConfig
{
    [JsonPropertyName("cmds")]
    public string Commands { get; set; } = "css_hidemodels;css_hide";

    [JsonPropertyName("who_hidden")]
    public string WhoHidden { get; set; } = "@all";

    [JsonIgnore]
    public HideMode Mode { get; set; }

    [JsonPropertyName("ConfigVersion")]
    public override int Version { get; set; } = 3;
}