using GameNetcodeStuff;
using System;
using System.Linq;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

namespace com.github.zehsteam.Whiteboard.Helpers;

internal static class PlayerUtils
{
    public static PlayerControllerB LocalPlayerScript => GameNetworkManager.Instance?.localPlayerController ?? null;

    public static PlayerControllerB[] AllPlayerScripts => StartOfRound.Instance?.allPlayerScripts ?? [];
    public static PlayerControllerB[] ConnectedPlayerScripts => [.. AllPlayerScripts.Where(IsConnected)];
    public static PlayerControllerB[] AlivePlayerScripts => [.. ConnectedPlayerScripts.Where(x => !x.isPlayerDead)];
    public static PlayerControllerB[] DeadPlayerScripts => [.. ConnectedPlayerScripts.Where(x => x.isPlayerDead)];

    public static bool TryGetLocalPlayerScript(out PlayerControllerB playerScript)
    {
        playerScript = LocalPlayerScript;
        return playerScript != null;
    }

    public static bool IsLocalPlayer(PlayerControllerB playerScript)
    {
        if (playerScript == null)
            return false;

        return playerScript == LocalPlayerScript;
    }

    public static bool IsConnected(PlayerControllerB playerScript)
    {
        if (playerScript == null)
            return false;

        return playerScript.isPlayerControlled || playerScript.isPlayerDead;
    }

    #region Get by
    // Client ID
    public static PlayerControllerB GetPlayerScriptByClientId(ulong clientId)
    {
        return ConnectedPlayerScripts.FirstOrDefault(playerScript => playerScript.actualClientId == clientId);
    }

    public static bool TryGetPlayerScriptByClientId(ulong clientId, out PlayerControllerB playerScript)
    {
        playerScript = GetPlayerScriptByClientId(clientId);
        return playerScript != null;
    }

    // Player Index
    public static PlayerControllerB GetPlayerScriptByPlayerId(int playerId)
    {
        if (playerId < 0 || playerId > ConnectedPlayerScripts.Length - 1)
            return null;

        return ConnectedPlayerScripts[playerId];
    }

    public static bool TryGetPlayerScriptByPlayerId(int playerId, out PlayerControllerB playerScript)
    {
        playerScript = GetPlayerScriptByPlayerId(playerId);
        return playerScript != null;
    }

    // Username
    public static PlayerControllerB GetPlayerScriptByUsername(string username)
    {
        PlayerControllerB[] playerScripts = [.. ConnectedPlayerScripts.OrderBy(x => x.playerUsername.Length)];

        PlayerControllerB targetPlayerScript = playerScripts.FirstOrDefault(x => x.playerUsername.Equals(username, StringComparison.OrdinalIgnoreCase));
        targetPlayerScript ??= playerScripts.FirstOrDefault(x => x.playerUsername.StartsWith(username, StringComparison.OrdinalIgnoreCase));
        targetPlayerScript ??= playerScripts.FirstOrDefault(x => x.playerUsername.Contains(username, StringComparison.OrdinalIgnoreCase));
        return targetPlayerScript;
    }

    public static bool TryGetPlayerScriptByUsername(string username, out PlayerControllerB playerScript)
    {
        playerScript = GetPlayerScriptByUsername(username);
        return playerScript != null;
    }
    #endregion

    // Random
    public static PlayerControllerB GetRandomPlayerScript(PlayerControllerB[] playerScripts, bool excludeLocal = false)
    {
        if (playerScripts == null || playerScripts.Length == 0)
            return null;

        PlayerControllerB[] filteredPlayerScripts = [.. playerScripts.Where(playerScript =>
        {
            if (!excludeLocal)
                return true;

            return !IsLocalPlayer(playerScript);
        })];

        if (filteredPlayerScripts.Length == 0)
            return null;

        return filteredPlayerScripts[Random.Range(0, filteredPlayerScripts.Length)];
    }

    public static bool TryGetRandomPlayerScript(PlayerControllerB[] playerScripts, out PlayerControllerB playerScript, bool excludeLocal = false)
    {
        playerScript = GetRandomPlayerScript(playerScripts, excludeLocal);
        return playerScript != null;
    }



    public static void SetControlsEnabled(bool value)
    {
        if (value)
        {
            EnableControls();
        }
        else
        {
            DisableControls();
        }
    }

    private static void EnableControls()
    {
        if (!TryGetLocalPlayerScript(out PlayerControllerB playerScript))
            return;

        playerScript.disableMoveInput = false;

        InputActionAsset actions = IngamePlayerSettings.Instance.playerInput.actions;

        try
        {
            // PlayerControllerB
            playerScript.playerActions.Movement.Look.performed += playerScript.Look_performed;
            actions.FindAction("Jump").performed += playerScript.Jump_performed;
            actions.FindAction("Crouch").performed += playerScript.Crouch_performed;
            actions.FindAction("Interact").performed += playerScript.Interact_performed;
            actions.FindAction("ItemSecondaryUse").performed += playerScript.ItemSecondaryUse_performed;
            actions.FindAction("ItemTertiaryUse").performed += playerScript.ItemTertiaryUse_performed;
            actions.FindAction("ActivateItem").performed += playerScript.ActivateItem_performed;
            actions.FindAction("ActivateItem").canceled += playerScript.ActivateItem_canceled;
            actions.FindAction("Discard").performed += playerScript.Discard_performed;
            actions.FindAction("SwitchItem").performed += playerScript.ScrollMouse_performed;
            //actions.FindAction("OpenMenu").performed += playerScript.OpenMenu_performed;
            actions.FindAction("InspectItem").performed += playerScript.InspectItem_performed;
            actions.FindAction("SpeedCheat").performed += playerScript.SpeedCheat_performed;
            actions.FindAction("Emote1").performed += playerScript.Emote1_performed;
            actions.FindAction("Emote2").performed += playerScript.Emote2_performed;

            playerScript.isTypingChat = false;

            // HUDManager
            actions.FindAction("EnableChat").performed += HUDManager.Instance.EnableChat_performed;
            //actions.FindAction("OpenMenu").performed += HUDManager.Instance.OpenMenu_performed;
            actions.FindAction("SubmitChat").performed += HUDManager.Instance.SubmitChat_performed;
            actions.FindAction("PingScan").performed += HUDManager.Instance.PingScan_performed;

            playerScript.playerActions.Movement.Enable();
        }
        catch (Exception e)
        {
            Logger.LogError($"Error while subscribing to input in PlayerController\n\n{e}");
        }

        playerScript.playerActions.Movement.Enable();
    }

    private static void DisableControls()
    {
        if (!TryGetLocalPlayerScript(out PlayerControllerB playerScript))
            return;

        playerScript.disableMoveInput = true;

        InputActionAsset actions = IngamePlayerSettings.Instance.playerInput.actions;

        try
        {
            // PlayerControllerB
            playerScript.playerActions.Movement.Look.performed -= playerScript.Look_performed;
            actions.FindAction("Jump").performed -= playerScript.Jump_performed;
            actions.FindAction("Crouch").performed -= playerScript.Crouch_performed;
            actions.FindAction("Interact").performed -= playerScript.Interact_performed;
            actions.FindAction("ItemSecondaryUse").performed -= playerScript.ItemSecondaryUse_performed;
            actions.FindAction("ItemTertiaryUse").performed -= playerScript.ItemTertiaryUse_performed;
            actions.FindAction("ActivateItem").performed -= playerScript.ActivateItem_performed;
            actions.FindAction("ActivateItem").canceled -= playerScript.ActivateItem_canceled;
            actions.FindAction("Discard").performed -= playerScript.Discard_performed;
            actions.FindAction("SwitchItem").performed -= playerScript.ScrollMouse_performed;
            //actions.FindAction("OpenMenu").performed -= playerScript.OpenMenu_performed;
            actions.FindAction("InspectItem").performed -= playerScript.InspectItem_performed;
            actions.FindAction("SpeedCheat").performed -= playerScript.SpeedCheat_performed;
            actions.FindAction("Emote1").performed -= playerScript.Emote1_performed;
            actions.FindAction("Emote2").performed -= playerScript.Emote2_performed;

            playerScript.isTypingChat = true;

            // HUDManager
            actions.FindAction("EnableChat").performed -= HUDManager.Instance.EnableChat_performed;
            //actions.FindAction("OpenMenu").performed -= HUDManager.Instance.OpenMenu_performed;
            actions.FindAction("SubmitChat").performed -= HUDManager.Instance.SubmitChat_performed;
            actions.FindAction("PingScan").performed -= HUDManager.Instance.PingScan_performed;

            playerScript.playerActions.Movement.Disable();
        }
        catch (Exception e)
        {
            Logger.LogError($"Error while unsubscribing to input in PlayerController\n\n{e}");
        }

        playerScript.playerActions.Movement.Disable();
    }
}
