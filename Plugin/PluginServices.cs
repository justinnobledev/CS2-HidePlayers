using CounterStrikeSharp.API.Core;
using Microsoft.Extensions.DependencyInjection;

namespace HidePlayers;

public sealed class PluginServices : IPluginServiceCollection<Plugin>
{
    public void ConfigureServices(IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<PlayerManager>();
        serviceCollection.AddSingleton<CommandManager>();
        serviceCollection.AddSingleton<HideManager>();
    }
}