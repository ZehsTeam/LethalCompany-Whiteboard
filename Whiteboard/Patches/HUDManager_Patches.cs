using com.github.zehsteam.Whiteboard.MonoBehaviours;
using HarmonyLib;

namespace com.github.zehsteam.Whiteboard.Patches;

[HarmonyPatch(typeof(HUDManager))]
internal static class HUDManager_Patches
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

    [HarmonyPatch(nameof(HUDManager.EnableChat_performed))]
    [HarmonyPrefix]
    private static bool EnableChat_performed_Patch()
    {
        if (WhiteboardEditor.Instance == null)
            return true;

        return !WhiteboardEditor.Instance.IsWindowOpen;
    }

    [HarmonyPatch(nameof(HUDManager.SubmitChat_performed))]
    [HarmonyPrefix]
    private static bool SubmitChat_performed_Patch()
    {
        if (WhiteboardEditor.Instance == null)
            return true;

        return !WhiteboardEditor.Instance.IsWindowOpen;
    }
}
