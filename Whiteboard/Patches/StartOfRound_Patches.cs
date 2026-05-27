using com.github.zehsteam.Whiteboard.Helpers;
using com.github.zehsteam.Whiteboard.Managers;
using com.github.zehsteam.Whiteboard.MonoBehaviours;
using GameNetcodeStuff;
using HarmonyLib;
using Unity.Netcode;
using UnityEngine;

namespace com.github.zehsteam.Whiteboard.Patches;

[HarmonyPatch(typeof(StartOfRound))]
internal static class StartOfRound_Patches
{
    [HarmonyPatch(nameof(StartOfRound.Awake))]
    [HarmonyPostfix]
    private static void Awake_Patch()
    {
        SpawnPluginNetworkHandler();
    }

    private static void SpawnPluginNetworkHandler()
    {
        if (!NetworkUtils.IsServer)
            return;

        var networkHandlerHost = Object.Instantiate(Assets.PluginNetworkHandlerPrefab, Vector3.zero, Quaternion.identity);
        networkHandlerHost.GetComponent<NetworkObject>().Spawn();
    }

    [HarmonyPatch(nameof(StartOfRound.OnClientConnect))]
    [HarmonyPrefix]
    private static void OnClientConnect_Patch(ref ulong clientId)
    {
        var clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = [clientId]
            }
        };

        PluginNetworkHandler.Instance?.SetWhiteboardUnlockablePrice_ClientRpc(ConfigManager.Whiteboard_Price.Value, clientRpcParams);
    }

    [HarmonyPatch(nameof(StartOfRound.ReviveDeadPlayers))]
    [HarmonyPostfix]
    private static void ReviveDeadPlayers_Patch()
    {
        if (WhiteboardEditor.Instance == null)
            return;

        if (!WhiteboardEditor.Instance.IsWindowOpen)
            return;

        if (PlayerUtils.TryGetLocalPlayerScript(out PlayerControllerB playerScript))
        {
            playerScript.disableMoveInput = true;
        }
    }
}
