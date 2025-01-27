using CounterStrikeSharp.API.Core;
using Microsoft.Extensions.Logging;

namespace HidePlayers;

public sealed partial class Plugin : IPluginConfig<PluginConfig>
{
    public PluginConfig Config { get; set; } = new();

    public void OnConfigParsed(PluginConfig config)
    {
        if (config.Version < Config.Version)
        {
            Logger.LogWarning("Your config version is outdated. (v. {old} -> v. {new})", config.Version, Config.Version);
        }

        Config = config;
        config.Mode = ParseHideMode(config.WhoHidden);
    }
}