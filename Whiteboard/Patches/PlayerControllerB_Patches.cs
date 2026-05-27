using com.github.zehsteam.Whiteboard.Helpers;
using com.github.zehsteam.Whiteboard.MonoBehaviours;
using GameNetcodeStuff;
using HarmonyLib;

namespace com.github.zehsteam.Whiteboard.Patches;

[HarmonyPatch(typeof(PlayerControllerB))]
internal static class PlayerControllerB_Patches
{
    [HarmonyPatch(nameof(PlayerControllerB.Start))]
    [HarmonyPostfix]
    private static void Start_Patch(ref PlayerControllerB __instance)
    {
        if (!PlayerUtils.IsLocalPlayer(__instance))
            return;

        MonoBehaviours.Whiteboard.Instance?.SetWorldCanvasCamera();
    }

    [HarmonyPatch(nameof(PlayerControllerB.KillPlayer))]
    [HarmonyPostfix]
    private static void KillPlayer_Patch()
    {
        if (WhiteboardEditor.Instance == null)
            return;

        if (WhiteboardEditor.Instance.IsWindowOpen)
        {
            WhiteboardEditor.Instance.CloseWindow();
        }
    }
}
