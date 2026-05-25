using BepInEx.Configuration;
using com.github.zehsteam.Whiteboard.Helpers;
using com.github.zehsteam.Whiteboard.MonoBehaviours;

namespace com.github.zehsteam.Whiteboard.Managers;

internal static class ConfigManager
{
    public static ConfigFile ConfigFile { get; private set; }

    // Misc
    public static ConfigEntry<bool> Misc_ExtendedLogging { get; private set; }

    // Whiteboard
    public static ConfigEntry<int> Whiteboard_Price { get; private set; }
    public static ConfigEntry<bool> Whiteboard_HostOnlyEdit { get; private set; }
    public static ConfigEntry<string> Whiteboard_DefaultDisplayText { get; private set; }

    public static void Initialize(ConfigFile configFile)
    {
        ConfigFile = configFile;
        BindConfigs();
    }

    private static void BindConfigs()
    {
        ConfigHelper.SkipAutoGen();

        // Misc
        Misc_ExtendedLogging = ConfigHelper.Bind("Misc", "ExtendedLogging", defaultValue: false, "Enable extended logging.");

        // Whiteboard
        Whiteboard_Price =              ConfigHelper.Bind("Whiteboard", "Price",              defaultValue: 100,  "The price of the whiteboard in the store.",
            acceptableValues: new AcceptableValueRange<int>(0, 500));
        Whiteboard_HostOnlyEdit =       ConfigHelper.Bind("Whiteboard", "HostOnlyEdit",       defaultValue: false, "If enabled, only the host can edit the whiteboard.");
        Whiteboard_DefaultDisplayText = ConfigHelper.Bind("Whiteboard", "DefaultDisplayText", defaultValue: "",    "The default display text that shows on the whiteboard. Supports rich text tags.");

        Whiteboard_Price.SettingChanged += (_, _) => Whiteboard_Price_SettingChanged();
        Whiteboard_HostOnlyEdit.SettingChanged += (_, _) => Whiteboard_HostOnly_SettingChanged();
    }

    private static void Whiteboard_Price_SettingChanged()
    {
        if (!NetworkUtils.IsServer)
            return;

        PluginNetworkBehaviour.Instance?.SetWhiteboardUnlockablePriceClientRpc(Whiteboard_Price.Value);
    }

    private static void Whiteboard_HostOnly_SettingChanged()
    {
        if (!NetworkUtils.IsServer)
            return;

        WhiteboardBehaviour.Instance?.IsHostOnly.Value = Whiteboard_HostOnlyEdit.Value;
    }
}
