using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Plugin;

namespace HidePlayers;

public sealed class PlayerManager(IPluginContext pluginContext) : Dictionary<int, bool>
{
    private Plugin _plugin = null!;

    private enum Action
    {
        Connect,
        Disconnect
    }

    public void Init(bool hotReload)
    {
        _plugin = (pluginContext.Plugin as Plugin)!;

        _plugin.RegisterListener<Listeners.OnClientPutInServer>(slot => OnPlayerAction(slot, Action.Connect));
        _plugin.RegisterListener<Listeners.OnClientDisconnect>(slot => OnPlayerAction(slot, Action.Disconnect));

        if (hotReload)
        {
            foreach (var player in Utilities.GetPlayers())
            {
                this.AddPlayer(player);
            }
        }
    }

    private void OnPlayerAction(int slot, Action action)
    {
        var player = Utilities.GetPlayerFromSlot(slot);

        if (player == null || !player.IsValid) return;

        if (action == Action.Connect)
        {
            this.AddPlayer(player);
        }
        else
        {
            if (!player.IsBot && _plugin.cookieId != -1 && ContainsKey(slot))
            {
                _plugin.cookieApi.SetPlayerCookie(player, _plugin.cookieId, base[slot].ToString());
            }

            this.RemovePlayer(player);
        }
    }

    public bool AddPlayer(CCSPlayerController player, bool value = default) => base.TryAdd(player.Slot, value);
    public bool RemovePlayer(CCSPlayerController player) => base.Remove(player.Slot);
}