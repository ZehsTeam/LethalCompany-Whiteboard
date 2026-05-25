using com.github.zehsteam.Whiteboard.MonoBehaviours;
using HarmonyLib;

namespace com.github.zehsteam.Whiteboard.Patches;

[HarmonyPatch(typeof(ShipBuildModeManager))]
internal class ShipBuildModeManager_Patches
{
    [HarmonyPatch(nameof(ShipBuildModeManager.PlayerMeetsConditionsToBuild))]
    [HarmonyPostfix]
    private static void PlayerMeetsConditionsToBuild_Patch(ref bool __result)
    {
        if (WhiteboardEditor.Instance == null)
            return;

        if (WhiteboardEditor.Instance.IsWindowOpen)
        {
            __result = false;
        }
    }
}
