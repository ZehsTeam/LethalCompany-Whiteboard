using com.github.zehsteam.Whiteboard.Helpers;
using com.github.zehsteam.Whiteboard.Managers;
using com.github.zehsteam.Whiteboard.MonoBehaviours;
using GameNetcodeStuff;
using HarmonyLib;
using Unity.Netcode;
using UnityEngine;

namespace com.github.zehsteam.Whiteboard.Patches;

[HarmonyPatch(typeof(StartOfRound))]
internal class StartOfRoundPatch
{
    [HarmonyPatch(nameof(StartOfRound.Awake))]
    [HarmonyPostfix]
    private static void AwakePatch()
    {
        SpawnNetworkHandler();
    }

    private static void SpawnNetworkHandler()
    {
        if (!NetworkUtils.IsServer) return;

        var networkHandlerHost = Object.Instantiate(Assets.PluginNetworkHandlerPrefab, Vector3.zero, Quaternion.identity);
        networkHandlerHost.GetComponent<NetworkObject>().Spawn();
    }

    [HarmonyPatch(nameof(StartOfRound.OnClientConnect))]
    [HarmonyPrefix]
    private static void OnClientConnectPatch(ref ulong clientId)
    {
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = [clientId]
            }
        };

        PluginNetworkBehaviour.Instance.SetWhiteboardUnlockablePriceClientRpc(ConfigManager.Whiteboard_Price.Value, clientRpcParams);
    }

    [HarmonyPatch(nameof(StartOfRound.ReviveDeadPlayers))]
    [HarmonyPostfix]
    private static void ReviveDeadPlayersPatch()
    {
        if (WhiteboardEditorBehaviour.Instance == null)
            return;

        if (!WhiteboardEditorBehaviour.Instance.IsWindowOpen)
            return;

        if (PlayerUtils.TryGetLocalPlayerScript(out PlayerControllerB playerScript))
        {
            playerScript.disableMoveInput = true;
        }
    }
}
