using com.github.zehsteam.Whiteboard.Helpers;
using com.github.zehsteam.Whiteboard.Managers;
using com.github.zehsteam.Whiteboard.Objects;
using GameNetcodeStuff;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace com.github.zehsteam.Whiteboard.MonoBehaviours;

public class Whiteboard : NetworkBehaviour
{
    public static Whiteboard Instance { get; private set; }

    [SerializeField]
    private InteractTrigger _interactTrigger;

    [SerializeField]
    private Canvas _worldCanvas;

    [SerializeField]
    private TextMeshProUGUI _whiteboardText;

    [SerializeField]
    private TMP_FontAsset[] _fontAssetArray = [];

    [SerializeField]
    private SpriteSheetData _emotesSpriteSheetData;

    [HideInInspector]
    public NetworkVariable<bool> IsHostOnly = new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public WhiteboardData Data { get; private set; }

    public static float[] FontSizeArray { get; private set; }
    public static FontStyles[] FontStyleArray { get; private set; }

    static Whiteboard()
    {
        {
            int length = 36;
            float valueBase = 0.05f;
            float valueStep = 0.01f;

            List<float> fontSizeList = [];

            for (int i = 0; i < length; i++)
            {
                fontSizeList.Add(valueBase + (valueStep * i));
            }

            FontSizeArray = [.. fontSizeList];
        }

        FontStyleArray = [
            FontStyles.Normal,
            FontStyles.Bold,
            FontStyles.Italic,
            FontStyles.Underline,
            FontStyles.Strikethrough
        ];
    }

    private void Awake()
    {
        // Ensure there is only one instance of the Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Destroy duplicate object
            return;
        }

        Instance = this;

        Data = new WhiteboardData();
    }

    private void Start()
    {
        if (_emotesSpriteSheetData != null)
        {
            _whiteboardText.spriteAsset = _emotesSpriteSheetData.SpriteAsset;
        }

        if (NetworkUtils.IsServer)
        {
            LoadData();
        }
        else
        {
            RequestData_ServerRpc();
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
            _interactTrigger.interactable = false;
        }
    }

    public override void OnNetworkDespawn()
    {
        IsHostOnly.OnValueChanged -= OnIsHostOnlyChanged;

        if (WhiteboardEditor.Instance == null)
            return;

        if (WhiteboardEditor.Instance.IsWindowOpen)
        {
            WhiteboardEditor.Instance.CloseWindow();
        }
    }

    private void OnIsHostOnlyChanged(bool previous, bool current)
    {
        if (NetworkUtils.IsServer)
            return;

        _interactTrigger.interactable = !current;
    }

    public void OnInteract()
    {
        if (WhiteboardEditor.Instance == null)
        {
            Logger.LogError("Failed to open whiteboard editor window. WhiteboardEditorBehaviour instance was not found.");
            return;
        }

        WhiteboardEditor.Instance.OpenWindow();
    }

    public void SetWorldCanvasCamera()
    {
        if (!PlayerUtils.TryGetLocalPlayerScript(out PlayerControllerB playerScript))
        {
            Logger.LogWarning("Failed to set whiteboard world canvas camera. Could not find the local player script or the local player is not spawned yet.");
            return;
        }
        
        _worldCanvas.worldCamera = playerScript.gameplayCamera;

        Logger.LogInfo("Set whiteboard world canvas camera.", extended: true);
    }

    public void LoadData()
    {
        if (!NetworkUtils.IsServer)
            return;

        if (!GameSaveFileHelper.IsSaveFileCreated())
        {
            SetData(new WhiteboardData());
            return;
        }

        string displayText = GameSaveFileHelper.Load("Whiteboard_DisplayText", defaultValue: ConfigManager.Whiteboard_DefaultDisplayText.Value);
        string textHexColor = GameSaveFileHelper.Load("Whiteboard_TextHexColor", defaultValue: WhiteboardEditor.DefaultTextHexColor);
        int fontSizeIndex = GameSaveFileHelper.Load("Whiteboard_FontSizeIndex", defaultValue: WhiteboardEditor.DefaultFontSizeIndex);
        int fontStyleIndex = GameSaveFileHelper.Load("Whiteboard_FontStyleIndex", defaultValue: 0);
        int fontFamilyIndex = GameSaveFileHelper.Load("Whiteboard_FontFamilyIndex", defaultValue: 0);
        int horizontalAlignmentIndex = GameSaveFileHelper.Load("Whiteboard_HorizontalAlignmentIndex", defaultValue: 0);
        int verticalAlignmentIndex = GameSaveFileHelper.Load("Whiteboard_VerticalAlignmentIndex", defaultValue: 0);

        SetData(new WhiteboardData(displayText, textHexColor, fontSizeIndex, fontStyleIndex, fontFamilyIndex, horizontalAlignmentIndex, verticalAlignmentIndex));
    }

    public void SaveData()
    {
        if (!NetworkUtils.IsServer)
            return;

        if (!GameSaveFileHelper.IsSaveFileCreated())
            return;

        GameSaveFileHelper.Save("Whiteboard_DisplayText", Data.DisplayText);
        GameSaveFileHelper.Save("Whiteboard_TextHexColor", Data.TextHexColor);
        GameSaveFileHelper.Save("Whiteboard_FontSizeIndex", Data.FontSizeIndex);
        GameSaveFileHelper.Save("Whiteboard_FontStyleIndex", Data.FontStyleIndex);
        GameSaveFileHelper.Save("Whiteboard_FontFamilyIndex", Data.FontFamilyIndex);
        GameSaveFileHelper.Save("Whiteboard_HorizontalAlignmentIndex", Data.HorizontalAlignmentIndex);
        GameSaveFileHelper.Save("Whiteboard_VerticalAlignmentIndex", Data.VerticalAlignmentIndex);
    }

    public void SetData(WhiteboardData data)
    {
        SetData_ServerRpc(data, NetworkUtils.LocalClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetData_ServerRpc(WhiteboardData data, ulong senderClientId)
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

        SetData_ClientRpc(data);
        SetData_Local(data);
    }
    
    [ClientRpc]
    private void SetData_ClientRpc(WhiteboardData data)
    {
        if (NetworkUtils.IsServer)
            return;

        SetData_Local(data);
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestData_ServerRpc(ServerRpcParams serverRpcParams = default)
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

        RequestData_ClientRpc(Data, clientRpcParams);
    }

    [ClientRpc]
    private void RequestData_ClientRpc(WhiteboardData data, ClientRpcParams clientRpcParams = default)
    {
        Logger.LogInfo("Recieved whiteboard data.", extended: true);

        SetData_Local(data);
    }

    public void SetData_Local(WhiteboardData data)
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

        if (_emotesSpriteSheetData != null)
        {
            displayText += _emotesSpriteSheetData.GetParsedText(Data.DisplayText);
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

        _whiteboardText.text = displayText;
        _whiteboardText.fontSize = FontSizeArray[Data.FontSizeIndex];
        _whiteboardText.fontStyle = FontStyleArray[Data.FontStyleIndex];
        _whiteboardText.font = _fontAssetArray[Data.FontFamilyIndex];

        _whiteboardText.horizontalAlignment = Data.HorizontalAlignmentIndex switch
        {
            0 => HorizontalAlignmentOptions.Left,
            1 => HorizontalAlignmentOptions.Center,
            2 => HorizontalAlignmentOptions.Right,
            _ => HorizontalAlignmentOptions.Left,
        };

        _whiteboardText.verticalAlignment = Data.VerticalAlignmentIndex switch
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
        if (!Logger.IsExtendedLoggingEnabled)
            return;

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
