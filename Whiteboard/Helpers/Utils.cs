using BepInEx;
using BepInEx.Configuration;
using GameNetcodeStuff;
using System.IO;
using UnityEngine;

namespace com.github.zehsteam.Whiteboard.Helpers;

internal static class Utils
{
    public static string GetPluginDirectoryPath()
    {
        return Path.GetDirectoryName(Plugin.Instance.Info.Location);
    }

    public static string GetConfigDirectoryPath()
    {
        return Paths.ConfigPath;
    }

    public static string GetPluginPersistentDataPath()
    {
        return Path.Combine(Application.persistentDataPath, MyPluginInfo.PLUGIN_NAME);
    }

    public static ConfigFile CreateConfigFile(BaseUnityPlugin plugin, string path, string name = null, bool saveOnInit = false)
    {
        BepInPlugin metadata = MetadataHelper.GetMetadata(plugin);
        name ??= metadata.GUID;
        name += ".cfg";
        return new ConfigFile(Path.Combine(path, name), saveOnInit, metadata);
    }

    public static ConfigFile CreateLocalConfigFile(BaseUnityPlugin plugin, string name = null, bool saveOnInit = false)
    {
        return CreateConfigFile(plugin, GetConfigDirectoryPath(), name, saveOnInit);
    }

    public static ConfigFile CreateGlobalConfigFile(BaseUnityPlugin plugin, string name = null, bool saveOnInit = false)
    {
        string path = GetPluginPersistentDataPath();
        name ??= "global";
        return CreateConfigFile(plugin, path, name, saveOnInit);
    }

    public static bool RollPercentChance(float percent)
    {
        if (percent <= 0f) return false;
        if (percent >= 100f) return true;
        return Random.value * 100f <= percent;
    }



    public static void SetCursorLockState(bool value)
    {
        // If the pause menu is open and you try to lock the cursor, return.
        if (IsQuickMenuOpen()) return;

        Cursor.lockState = value ? CursorLockMode.Locked : CursorLockMode.None;

        if (value)
        {
            Cursor.visible = false;
        }
        else
        {
            if (!StartOfRound.Instance.localPlayerUsingController)
            {
                Cursor.visible = true;
            }
        }
    }

    public static string GetCurrentSaveFileName()
    {
        return GameNetworkManager.Instance.currentSaveFileName;
    }

    public static void SaveToCurrentSaveFile<T>(string key, T value)
    {
        ES3.Save($"{MyPluginInfo.PLUGIN_GUID}.{key}", value, GetCurrentSaveFileName());
    }

    public static T LoadFromCurrentSaveFile<T>(string key, T defaultValue = default)
    {
        return ES3.Load($"{MyPluginInfo.PLUGIN_GUID}.{key}", GetCurrentSaveFileName(), defaultValue);
    }

    public static bool KeyExistsInCurrentSaveFile(string key)
    {
        return ES3.KeyExists($"{MyPluginInfo.PLUGIN_GUID}.{key}", GetCurrentSaveFileName());
    }

    public static bool IsQuickMenuOpen()
    {
        if (!PlayerUtils.TryGetLocalPlayerScript(out PlayerControllerB playerScript))
            return false;

        return playerScript.quickMenuManager.isMenuOpen;
    }
}
