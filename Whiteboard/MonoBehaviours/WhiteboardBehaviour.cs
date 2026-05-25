using com.github.zehsteam.Whiteboard.Helpers;
using com.github.zehsteam.Whiteboard.Managers;
using com.github.zehsteam.Whiteboard.Objects;
using GameNetcodeStuff;
using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace com.github.zehsteam.Whiteboard.MonoBehaviours;

public class WhiteboardBehaviour : NetworkBehaviour
{
    public static WhiteboardBehaviour Instance;

    public InteractTrigger InteractTrigger;
    public Canvas WorldCanvas = null;
    public TextMeshProUGUI WhiteboardText = null;
    public float[] FontSizeArray = [];
    public FontStyles[] FontStyleArray = [];
    public TMP_FontAsset[] FontAssetArray = [];
    public SpriteSheetData EmotesSpriteSheetData = null;

    [HideInInspector]
    public NetworkVariable<bool> IsHostOnly = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public WhiteboardData Data {  get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;

        Data = new WhiteboardData();
    }

    private void Start()
    {
        if (EmotesSpriteSheetData != null)
        {
            WhiteboardText.spriteAsset = EmotesSpriteSheetData.SpriteAsset;
        }

        if (NetworkUtils.IsServer)
        {
            LoadData();
        }
        else
        {
            RequestDataServerRpc();
        }
    }

    public override void OnNetworkSpawn()
    {
        IsHostOnly.OnValueChanged += OnIsHostOnlyChanged;

        if (NetworkUtils.IsServer)
        {
            IsHostOnly.Value = ConfigManager.Whiteboard_HostOnlyEdit.Value;
        }
        else if (IsHostOnly.Value)
        {
            InteractTrigger.interactable = false;
        }
    }

    public override void OnNetworkDespawn()
    {
        IsHostOnly.OnValueChanged -= OnIsHostOnlyChanged;

        if (WhiteboardEditorBehaviour.Instance == null) return;

        if (WhiteboardEditorBehaviour.Instance.IsWindowOpen)
        {
            WhiteboardEditorBehaviour.Instance.CloseWindow();
        }
    }

    private void OnIsHostOnlyChanged(bool previous, bool current)
    {
        if (NetworkUtils.IsServer) return;

        InteractTrigger.interactable = !current;
    }

    public void OnInteract()
    {
        if (WhiteboardEditorBehaviour.Instance == null)
        {
            Logger.LogError("Failed to open whiteboard editor window. WhiteboardEditorBehaviour instance was not found.");
            return;
        }

        WhiteboardEditorBehaviour.Instance.OpenWindow();
    }

    public void SetWorldCanvasCamera()
    {
        if (!PlayerUtils.TryGetLocalPlayerScript(out PlayerControllerB playerScript))
        {
            Logger.LogWarning("Failed to set whiteboard world canvas camera. Could not find the local player script or the local player is not spawned yet.");
            return;
        }
        
        WorldCanvas.worldCamera = playerScript.gameplayCamera;

        Logger.LogInfo("Set whiteboard world canvas camera.", extended: true);
    }

    private void LoadData()
    {
        if (!NetworkUtils.IsServer) return;

        string displayText = Utils.LoadFromCurrentSaveFile("Whiteboard_DisplayText", defaultValue: ConfigManager.Whiteboard_DefaultDisplayText.Value);
        string textHexColor = Utils.LoadFromCurrentSaveFile("Whiteboard_TextHexColor", defaultValue: WhiteboardEditorBehaviour.DefaultTextHexColor);
        int fontSizeIndex = Utils.LoadFromCurrentSaveFile("Whiteboard_FontSizeIndex", defaultValue: WhiteboardEditorBehaviour.DefaultFontSizeIndex);
        int fontStyleIndex = Utils.LoadFromCurrentSaveFile("Whiteboard_FontStyleIndex", defaultValue: 0);
        int fontFamilyIndex = Utils.LoadFromCurrentSaveFile("Whiteboard_FontFamilyIndex", defaultValue: 0);
        int horizontalAlignmentIndex = Utils.LoadFromCurrentSaveFile("Whiteboard_HorizontalAlignmentIndex", defaultValue: 0);
        int verticalAlignmentIndex = Utils.LoadFromCurrentSaveFile("Whiteboard_VerticalAlignmentIndex", defaultValue: 0);

        SetData(new WhiteboardData(displayText, textHexColor, fontSizeIndex, fontStyleIndex, fontFamilyIndex, horizontalAlignmentIndex, verticalAlignmentIndex));
    }

    private void SaveData()
    {
        if (!NetworkUtils.IsServer) return;

        Utils.SaveToCurrentSaveFile("Whiteboard_DisplayText", Data.DisplayText);
        Utils.SaveToCurrentSaveFile("Whiteboard_TextHexColor", Data.TextHexColor);
        Utils.SaveToCurrentSaveFile("Whiteboard_FontSizeIndex", Data.FontSizeIndex);
        Utils.SaveToCurrentSaveFile("Whiteboard_FontStyleIndex", Data.FontStyleIndex);
        Utils.SaveToCurrentSaveFile("Whiteboard_FontFamilyIndex", Data.FontFamilyIndex);
        Utils.SaveToCurrentSaveFile("Whiteboard_HorizontalAlignmentIndex", Data.HorizontalAlignmentIndex);
        Utils.SaveToCurrentSaveFile("Whiteboard_VerticalAlignmentIndex", Data.VerticalAlignmentIndex);
    }

    public void SetData(WhiteboardData data)
    {
        SetDataServerRpc(data, NetworkUtils.LocalClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetDataServerRpc(WhiteboardData data, ulong senderClientId)
    {
        if (NetworkUtils.IsLocalClientId(senderClientId))
        {
            // Host
            Logger.LogInfo($"Set the whiteboard data. Display text: \"{data.DisplayText}\"", extended: true);
        }
        else
        {
            // Client
            if (ConfigManager.Whiteboard_HostOnlyEdit.Value)
            {
                Logger.LogWarning($"Client #{senderClientId} tried to edit the whiteboard while HostOnly mode is enabled.");
                return;
            }

            Logger.LogInfo($"Client #{senderClientId} set the whiteboard data. Display text: \"{data.DisplayText}\".", extended: true);
        }

        SetDataClientRpc(data);
        SetDataOnLocalClient(data);
    }
    
    [ClientRpc]
    private void SetDataClientRpc(WhiteboardData data)
    {
        if (NetworkUtils.IsServer) return;

        SetDataOnLocalClient(data);
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestDataServerRpc(ServerRpcParams serverRpcParams = default)
    {
        var senderClientId = serverRpcParams.Receive.SenderClientId;

        Logger.LogInfo($"Recieved request for whiteboard data from client #{senderClientId}", extended: true);

        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = [senderClientId]
            }
        };

        RequestDataClientRpc(Data, clientRpcParams);
    }

    [ClientRpc]
    private void RequestDataClientRpc(WhiteboardData data, ClientRpcParams clientRpcParams = default)
    {
        Logger.LogInfo("Recieved whiteboard data.", extended: true);

        SetDataOnLocalClient(data);
    }

    public void SetDataOnLocalClient(WhiteboardData data)
    {
        Data = data;
        SaveData();
        UpdateWorldCanvas();
        LogDataExtended();
    }
    
    #region WorldCanvas
    private void UpdateWorldCanvas()
    {
        UpdateWhiteboardText();
    }

    private void UpdateWhiteboardText()
    {
        if (Data == null)
        {
            Logger.LogWarning("WhiteboardData is null in WhiteboardBehaviour.UpdateWhiteboardText(); Setting WhiteboardData to default.");

            Data = new WhiteboardData();
        }

        string displayText = string.Empty;

        if (!string.IsNullOrWhiteSpace(Data.TextHexColor))
        {
            displayText += $"<color={Data.TextHexColor}>";
        }

        if (EmotesSpriteSheetData != null)
        {
            displayText += EmotesSpriteSheetData.GetParsedText(Data.DisplayText);
        }
        else
        {
            displayText += Data.DisplayText;
        }

        // If using the Signal Translator font, make all the text lowercase.
        if (Data.FontFamilyIndex == 2)
        {
            displayText = displayText.ToLower();
        }

        WhiteboardText.text = displayText;
        WhiteboardText.fontSize = FontSizeArray[Data.FontSizeIndex];
        WhiteboardText.fontStyle = FontStyleArray[Data.FontStyleIndex];
        WhiteboardText.font = FontAssetArray[Data.FontFamilyIndex];

        WhiteboardText.horizontalAlignment = Data.HorizontalAlignmentIndex switch
        {
            0 => HorizontalAlignmentOptions.Left,
            1 => HorizontalAlignmentOptions.Center,
            2 => HorizontalAlignmentOptions.Right,
            _ => HorizontalAlignmentOptions.Left,
        };

        WhiteboardText.verticalAlignment = Data.VerticalAlignmentIndex switch
        {
            0 => VerticalAlignmentOptions.Top,
            1 => VerticalAlignmentOptions.Middle,
            2 => VerticalAlignmentOptions.Bottom,
            _ => VerticalAlignmentOptions.Top,
        };
    }
    #endregion

    private void LogDataExtended()
    {
        string message = string.Empty;

        message += $"DisplayText: \n\"{Data.DisplayText}\"\n\n";
        message += $"TextHexColor: \"{Data.TextHexColor}\"\n";
        message += $"FontSizeIndex: {Data.FontSizeIndex}\n";
        message += $"FontStyleIndex: {Data.FontStyleIndex}\n";
        message += $"FontFamilyIndex: {Data.FontFamilyIndex}\n";
        message += $"HorizontalAlignmentIndex: {Data.HorizontalAlignmentIndex}\n";
        message += $"VerticalAlignmentIndex: {Data.VerticalAlignmentIndex}\n";

        Logger.LogInfo($"\n{message.Trim()}\n\n", extended: true);
    }
}
