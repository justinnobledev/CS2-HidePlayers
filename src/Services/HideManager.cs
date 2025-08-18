using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Plugin;

namespace HidePlayers;

public sealed class HideManager(IPluginContext pluginContext, PlayerManager playerManager)
{
    private Plugin plugin_ = null!;

    public void Init()
    {
        plugin_ = (pluginContext.Plugin as Plugin)!;

        plugin_.RegisterListener<Listeners.CheckTransmit>(OnCheckTransmit);
    }

    private void OnCheckTransmit(CCheckTransmitInfoList infoList)
    {
        foreach (var (info, player) in infoList)
        {
            if (player == null || player.Connected != PlayerConnectedState.PlayerConnected || !playerManager.TryGetValue(player.Slot, out bool isEnableHide))
                continue;

            foreach (var slot in playerManager.Keys)
            {
                var target = Utilities.GetPlayerFromSlot(slot);

                if (target == null || !target.IsValid) continue;

                var targetPawn = target.PlayerPawn.Value;

                if (targetPawn == null) continue;

                if (targetPawn.LifeState != (byte)LifeState_t.LIFE_DEAD || targetPawn.LifeState != (byte)LifeState_t.LIFE_DYING)
                {
                    if (target.Slot == player.Slot) continue;

                    if (player.Pawn.Value?.As<CCSPlayerPawnBase>().PlayerState == CSPlayerState.STATE_OBSERVER_MODE) continue;
                }

                if (isEnableHide && (plugin_.Config.Mode == HideMode.All || plugin_.Config.Mode == HideMode.Team && player.Team == target.Team || plugin_.Config.Mode == HideMode.Enemy && player.Team != target.Team))
                {
                    info.TransmitEntities.Remove(targetPawn.Index);
                }
            }
        }
    }

    public bool Toggle(CCSPlayerController player)
    {
        return playerManager[player.Slot] ^= true;
    }
}