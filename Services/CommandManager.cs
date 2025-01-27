using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Plugin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Extensions;
using Microsoft.Extensions.Logging;

namespace HidePlayers;

public sealed class CommandManager(ILogger<CommandManager> logger, IPluginContext pluginContext, HideManager hideManager)
{
    private Plugin plugin_ = null!;

    public void Init()
    {
        plugin_ = (pluginContext.Plugin as Plugin)!;

        var cmds = plugin_.Config.Commands
            .Split(';')
            .Select(c => c.Trim())
            .ToArray();

        foreach (var cmd in cmds)
        {
            plugin_.AddCommand(cmd, "Hide players models", OnToggleCommand);
        }

        plugin_.AddCommand("css_hide_reload", $"Reload `{plugin_.ModuleName}` configuration", OnConfigReload);
    }

    [RequiresPermissions("@css/root")]
    private void OnConfigReload(CCSPlayerController? player, CommandInfo commandInfo)
    {
        try
        {
            plugin_.Config.Update();
            plugin_.Config.Reload();

            plugin_.Config.Mode = plugin_.ParseHideMode(plugin_.Config.WhoHidden);

            commandInfo.ReplyToCommand($"[{plugin_.ModuleName}] You have successfully reloaded the config.");
        }
        catch (Exception ex)
        {
            commandInfo.ReplyToCommand($"[{plugin_.ModuleName}] An error occurred.");
            logger.LogError("An error occurred while reloading the configuration: {error}", ex.Message);
        }
    }

    private void OnToggleCommand(CCSPlayerController? player, CommandInfo commandInfo)
    {
        if (player == null) return;
 
        string tag = plugin_.Localizer["Plugin.Tag"];
        string status = plugin_.Localizer[hideManager.Toggle(player) ? "Plugin.Enable" : "Plugin.Disable"];

        player.PrintToChat(plugin_.Localizer["Player.Hide", tag, status]);
    }
}