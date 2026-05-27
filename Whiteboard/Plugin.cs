using BepInEx;
using com.github.zehsteam.Whiteboard.Dependencies.LethalConfigMod;
using com.github.zehsteam.Whiteboard.Helpers;
using com.github.zehsteam.Whiteboard.Managers;
using com.github.zehsteam.Whiteboard.Patches;
using HarmonyLib;

namespace com.github.zehsteam.Whiteboard;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency(LethalLib.Plugin.ModGUID)]
[BepInDependency(LethalConfigProxy.PLUGIN_GUID, BepInDependency.DependencyFlags.SoftDependency)]
internal class Plugin : BaseUnityPlugin
{
    private readonly Harmony _harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);

    internal static Plugin Instance { get; private set; }

    private void Awake()
    {
        Instance = this;

        Whiteboard.Logger.Initialize(BepInEx.Logging.Logger.CreateLogSource(MyPluginInfo.PLUGIN_GUID));
        Whiteboard.Logger.LogInfo($"{MyPluginInfo.PLUGIN_NAME} has awoken!");

        _harmony.PatchAll(typeof(GameNetworkManager_Patches));
        _harmony.PatchAll(typeof(StartOfRound_Patches));
        _harmony.PatchAll(typeof(HUDManager_Patches));
        _harmony.PatchAll(typeof(PlayerControllerB_Patches));
        _harmony.PatchAll(typeof(ShipBuildModeManager_Patches));

        ConfigManager.Initialize(Config);

        Assets.Load();

        RegisterUnlockableItems();

        NetworkUtils.NetcodePatcherAwake();
    }

    private void RegisterUnlockableItems()
    {
        UnlockableHelper.RegisterUnlockable(Assets.WhiteboardUnlockableItemDef, LethalLib.Modules.StoreType.Decor, price: ConfigManager.Whiteboard_Price.Value, Assets.WhiteboardBuyTerminalNode);
    }
}
