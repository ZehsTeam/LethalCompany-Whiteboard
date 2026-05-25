using com.github.zehsteam.Whiteboard.MonoBehaviours;
using HarmonyLib;

namespace com.github.zehsteam.Whiteboard.Patches;

[HarmonyPatch(typeof(HUDManager))]
internal class HUDManager_Patches
{
    [HarmonyPatch(nameof(HUDManager.Start))]
    [HarmonyPostfix]
    private static void Start_Patch()
    {
        WhiteboardEditor.Spawn();
    }

    [HarmonyPatch(nameof(HUDManager.OpenMenu_performed))]
    [HarmonyPrefix]
    private static void OpenMenu_performed_Patch()
    {
        if (WhiteboardEditor.Instance == null)
            return;

        if (WhiteboardEditor.Instance.IsWindowOpen)
        {
            WhiteboardEditor.Instance.CloseWindow();
        }
    }
}
