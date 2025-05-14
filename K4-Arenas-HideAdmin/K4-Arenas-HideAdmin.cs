using System.Text.Json.Serialization;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Capabilities;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using K4ArenaSharedApi;
using Microsoft.Extensions.Logging;

namespace K4_Arenas_HideAdmin;

public class SampleConfig : BasePluginConfig
{
    [JsonPropertyName("Permission")] public string Permission { get; set; } = "@css/kick";

}

public class K4_Arenas_HideAdmin : BasePlugin, IPluginConfig<SampleConfig>
{
    public override string ModuleName => "K4 Arenas HideAdmin";
    public override string ModuleVersion => "0.0.1";
    public required SampleConfig Config { get; set; }
    public static IK4ArenaSharedApi? SharedAPI_Arena { get; private set; }
    public (bool ArenaFound, bool Checked) ArenaSupport = (false, false);
    private List<int> hiddenAdmins = new List<int>();

    public void OnConfigParsed(SampleConfig config)
    {
        Config = config;
    }

    public override void Load(bool hotReload)
    {
        Console.WriteLine("Hello World!");
        AddCommandListener("css_hide", OnHide);
    }

    public void PerformAFKAction(CCSPlayerController player, bool afk)
    {
        if (!ArenaSupport.Checked)
        {
            string arenaPath = Path.GetFullPath(Path.Combine(ModuleDirectory, "..", "K4-Arenas"));
            ArenaSupport.ArenaFound = Directory.Exists(arenaPath);
            ArenaSupport.Checked = true;
        }

        if (!ArenaSupport.ArenaFound)
        {
            return;
        }

        if (SharedAPI_Arena is null)
        {
            PluginCapability<IK4ArenaSharedApi> Capability_SharedAPI = new("k4-arenas:sharedapi");
            SharedAPI_Arena = Capability_SharedAPI.Get();
        }
        SharedAPI_Arena?.PerformAFKAction(player, afk);
    }

    private HookResult OnHide(CCSPlayerController? player, CommandInfo commandInfo)
    {
        if (player == null || player.IsBot || player.IsHLTV || player.PlayerPawn.Value == null) return HookResult.Continue;
        var slot = player.Slot;
        if (!AdminManager.PlayerHasPermissions(player, Config.Permission)) return HookResult.Continue;
        if (!hiddenAdmins.Contains(slot))
        {
            hiddenAdmins.Add(slot);
            PerformAFKAction(player, true);
        }
        else
        {
            hiddenAdmins.Remove(slot);
            PerformAFKAction(player, false);
        }
        return HookResult.Continue;
    }
}
