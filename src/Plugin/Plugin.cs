using Clientprefs.API;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Capabilities;
using Microsoft.Extensions.Logging;

namespace HidePlayers;

public sealed partial class Plugin(PlayerManager playerManager, HideManager hideManager, CommandManager commandManager) : BasePlugin
{
    public override string ModuleName { get; } = "HidePlayers";
    public override string ModuleAuthor { get; } = "xstage";
    public override string ModuleVersion { get; } = "1.3.2";

    private readonly static PluginCapability<IClientprefsApi> PluginCapability = new("Clientprefs");

    internal IClientprefsApi cookieApi = null!;
    internal int cookieId = -1;

    public override void Load(bool hotReload)
    {
        playerManager.Init(hotReload);
        commandManager.Init();
        hideManager.Init();
    }

    public override void OnAllPluginsLoaded(bool hotReload)
    {
        cookieApi = PluginCapability.Get()!;

        cookieApi.OnDatabaseLoaded += OnDatabaseLoaded;
        cookieApi.OnPlayerCookiesCached += OnPlayerCookiesCached;

        if (!hotReload) return;
        
        foreach (var player in Utilities.GetPlayers().Where(p => !p.IsBot))
        {
            if(!playerManager.ContainsKey(player.Slot))
                playerManager.AddPlayer(player);
            
            if (!cookieApi.ArePlayerCookiesCached(player)) continue;
                
            playerManager[player.Slot] = bool.TryParse(cookieApi.GetPlayerCookie(player, cookieId), out var parsed) && parsed;
        }
    }

    public override void Unload(bool hotReload)
    {
        cookieApi.OnDatabaseLoaded -= OnDatabaseLoaded;
        cookieApi.OnPlayerCookiesCached -= OnPlayerCookiesCached;
    }

    private void OnDatabaseLoaded()
    {
        cookieId = cookieApi.RegPlayerCookie("hide_player_status", string.Empty, CookieAccess.CookieAccess_Public);

        if (cookieId == -1) Logger.LogError("Error during registration cookie");
    }

    private void OnPlayerCookiesCached(CCSPlayerController player)
    {
        if (cookieId == -1) return;

        playerManager[player.Slot] = bool.TryParse(cookieApi.GetPlayerCookie(player, cookieId), out var parsed) && parsed;
    }
}
