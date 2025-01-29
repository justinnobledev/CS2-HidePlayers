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

        StateTransition.Hook(Hook_StateTransitionPre, HookMode.Pre);
        VirtualFunctions.CBaseEntity_TakeDamageOldFunc.Hook(Hook_TakeDamageOld, HookMode.Pre);
    }

    public void Destroy()
    {
        StateTransition.Unhook(Hook_StateTransitionPre, HookMode.Pre);
        VirtualFunctions.CBaseEntity_TakeDamageOldFunc.Unhook(Hook_TakeDamageOld, HookMode.Pre);
    }

    private HookResult Hook_TakeDamageOld(DynamicHook hook)
    {
        var victim = hook.GetParam<CEntityInstance>(0);
        var info = hook.GetParam<CTakeDamageInfo>(1);

        if (victim.DesignerName != "player") return HookResult.Continue;

        var pawn = victim.As<CCSPlayerPawn>();

        if (pawn == null || !pawn.IsValid) return HookResult.Continue;

        if (info.DamageFlags.HasFlag(TakeDamageFlags_t.DFLAG_FORCE_DEATH))
        {
            info.DamageFlags &= ~TakeDamageFlags_t.DFLAG_FORCE_DEATH;
            StateTransition.Invoke(pawn, CSPlayerState.STATE_WELCOME);

            return HookResult.Changed;
        }

        return HookResult.Continue;
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

                if (targetPawn.LifeState != (byte)LifeState_t.LIFE_DEAD || targetPawn.LifeState != (byte)LifeState_t.LIFE_DYING)
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

    private HookResult Hook_StateTransitionPre(DynamicHook hook)
    {
        var player = hook.GetParam<CCSPlayerPawn>(0).OriginalController.Value;
        var state = hook.GetParam<CSPlayerState>(1);

        if (player == null || !playerManager.TryGetValue(player, out Player? data)) return HookResult.Continue;

        var oldPlayerState = player.Pawn.Value?.As<CCSPlayerPawnBase>().PlayerState;

        if (state != oldPlayerState)
        {
            Plugin.ForceFullUpdate(player);
        }

        return HookResult.Continue;
    }

    public bool Toggle(CCSPlayerController player)
    {
        if (!playerManager.TryGetValue(player, out Player? data)) return false;

        return data.isEnableHide ^= true;
    }
}