using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Plugin;
using CounterStrikeSharp.API.Modules.Events;

namespace HidePlayers;

public sealed class PlayerManager(IPluginContext pluginContext) : Dictionary<CCSPlayerController, Player>
{
    public void Init(bool hotReload)
    {
        var plugin = (pluginContext.Plugin as Plugin)!;

        plugin.RegisterEventHandler<EventPlayerConnectFull>(OnPlayerAction);
        plugin.RegisterEventHandler<EventPlayerDisconnect>(OnPlayerAction);

        if (hotReload)
        {
            foreach (var player in Plugin.GetPlayers())
            {
                TryAdd(player, new());
            }
        }
    }

    private HookResult OnPlayerAction<T>(T @event, GameEventInfo info) where T: GameEvent
    {
        var player = @event.Get<CCSPlayerController>("userid");

        if (!player.IsValid) return HookResult.Continue;

        if (@event is EventPlayerConnectFull)
        {
            TryAdd(player, new());
        }
        else if (@event is EventPlayerDisconnect)
        {
            Remove(player);
        }

        return HookResult.Continue;
    }
}