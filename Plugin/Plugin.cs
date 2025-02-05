using CounterStrikeSharp.API.Core;

namespace HidePlayers;

public sealed partial class Plugin(PlayerManager playerManager, HideManager hideManager, CommandManager commandManager) : BasePlugin
{
    public override string ModuleName { get; } = "HidePlayers";
    public override string ModuleAuthor { get; } = "xstage";
    public override string ModuleVersion { get; } = "1.2.5";

    public override void Load(bool hotReload)
    {
        playerManager.Init(hotReload);
        commandManager.Init();
        hideManager.Init();
    }

    public override void Unload(bool hotReload)
    {
        hideManager.Destroy();
    }
}
