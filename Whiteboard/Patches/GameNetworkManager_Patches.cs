using HarmonyLib;
using Unity.Netcode;
using UnityEngine;

namespace com.github.zehsteam.Whiteboard.Patches;

[HarmonyPatch(typeof(GameNetworkManager))]
internal class GameNetworkManager_Patches
{
    [HarmonyPatch(nameof(GameNetworkManager.Start))]
    [HarmonyPostfix]
    private static void Start_Patch()
    {
        AddNetworkPrefabs();
    }

    private static void AddNetworkPrefabs()
    {
        AddNetworkPrefab(Assets.PluginNetworkHandlerPrefab);
    }

    private static void AddNetworkPrefab(GameObject prefab)
    {
        if (prefab == null)
        {
            Logger.LogError($"Failed to add network prefab. GameObject is null.");
            return;
        }

        NetworkManager.Singleton.AddNetworkPrefab(prefab);

        Logger.LogInfo($"Registered \"{prefab.name}\" network prefab.");
    }
}
