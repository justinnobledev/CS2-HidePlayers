using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Plugin;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;

namespace HidePlayers;

public sealed class HideManager(IPluginContext pluginContext, PlayerManager playerManager)
{
    private Plugin plugin_ = null!;
    private MemoryFunctionVoid<CCSPlayerPawn, CSPlayerState> StateTransition = null!;

    public void Init()
    {
        StateTransition = new(GameData.GetSignature("StateTransition"));
        plugin_ = (pluginContext.Plugin as Plugin)!;

        plugin_.RegisterListener<Listeners.CheckTransmit>(OnCheckTransmit);

        StateTransition.Hook(Hook_StateTransition, HookMode.Post);
        VirtualFunctions.CBaseEntity_TakeDamageOldFunc.Hook(OnTakeDamageOld, HookMode.Pre);
    }

    public void Destroy()
    {
        StateTransition.Unhook(Hook_StateTransition, HookMode.Post);
        VirtualFunctions.CBaseEntity_TakeDamageOldFunc.Unhook(OnTakeDamageOld, HookMode.Pre);
    }

    private HookResult OnTakeDamageOld(DynamicHook hook)
    {
        var info = hook.GetParam<CTakeDamageInfo>(1);

        info.DamageFlags = TakeDamageFlags_t.DFLAG_NONE;

        return HookResult.Changed;
    }

    private void OnCheckTransmit(CCheckTransmitInfoList infoList)
    {
        var targets = Plugin.GetPlayers(false);

        foreach (var (info, player) in infoList)
        {
            if (player == null || player.Connected != PlayerConnectedState.PlayerConnected || !playerManager.TryGetValue(player, out Player? data))
                continue;

            foreach (var target in targets)
            {
                var targetPawn = target.PlayerPawn.Value;

                if (targetPawn == null) continue;

                if (targetPawn.LifeState == (byte)LifeState_t.LIFE_ALIVE)
                {
                    if (target.Slot == player.Slot) continue;

                    if (player.Pawn.Value?.As<CCSPlayerPawnBase>().PlayerState == CSPlayerState.STATE_OBSERVER_MODE) continue;
                }

                if (targetPawn.LifeState != (byte)LifeState_t.LIFE_ALIVE || data.isEnableHide && (plugin_.Config.Mode == HideMode.All || 
                plugin_.Config.Mode == HideMode.Team && player.Team == target.Team || plugin_.Config.Mode == HideMode.Enemy && player.Team != target.Team))
                {
                    info.TransmitEntities.Remove(targetPawn.Index);
                }
            }
        }
    }

    private HookResult Hook_StateTransition(DynamicHook hook)
    {
        var player = hook.GetParam<CCSPlayerPawn>(0).OriginalController.Value;
        var state = hook.GetParam<CSPlayerState>(1);

        if (player == null || !playerManager.TryGetValue(player, out Player? data)) return HookResult.Continue;

        if (state != data.oldPlayerState)
        {
            if (state == CSPlayerState.STATE_OBSERVER_MODE || data.oldPlayerState == CSPlayerState.STATE_OBSERVER_MODE)
            {
                Plugin.ForceFullUpdate(player);
            }
        }

        data.oldPlayerState = state;

        return HookResult.Continue;
    }

    public bool Toggle(CCSPlayerController player)
    {
        if (!playerManager.TryGetValue(player, out Player? data)) return false;

        return data.isEnableHide ^= true;
    }
}
