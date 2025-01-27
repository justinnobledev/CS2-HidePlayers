using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace HidePlayers;

public sealed partial class Plugin
{
    private static readonly INetworkServerService networkServerService = new();

    public HideMode ParseHideMode(string hideMode) => hideMode switch
    {
        "@all" => HideMode.All,
        "@team" => HideMode.Team,
        "@enemy" => HideMode.Enemy,
        _ => HideMode.All
    };

    public static List<CCSPlayerController> GetPlayers(bool skipBots = true) =>
        skipBots ? [.. Utilities.GetPlayers().Where(p => !p.IsBot)] : Utilities.GetPlayers();
    
    public static void ForceFullUpdate(CCSPlayerController? player)
    {
        if (player == null || !player.IsValid) return;

        var networkGameServer = networkServerService.GetIGameServer();
        networkGameServer.GetClientBySlot(player.Slot)?.ForceFullUpdate();

        player.PlayerPawn.Value?.Teleport(null, player.PlayerPawn.Value.EyeAngles, null);
    }
}