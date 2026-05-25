using com.github.zehsteam.Whiteboard.Helpers;
using com.github.zehsteam.Whiteboard.Managers;
using com.github.zehsteam.Whiteboard.Objects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace com.github.zehsteam.Whiteboard.MonoBehaviours;

public class WhiteboardEditorBehaviour : MonoBehaviour
{
    public static WhiteboardEditorBehaviour Instance;

    public GameObject EditorWindowObject = null;
    public TMP_InputField DisplayTextInputField = null;

    public GameObject HostOnlyObject = null;
    public Button HostOnlyButton = null;
    public GameObject HostOnlyCheckedObject = null;

    public Image TextColorPreviewImage = null;
    public TMP_Dropdown FontSizeDropdown = null;
    public TMP_Dropdown FontStyleDropdown = null;
    public TMP_Dropdown FontFamilyDropdown = null;
    public TMP_Dropdown HorizontalAlignmentDropdown = null;
    public TMP_Dropdown VerticalAlignmentDropdown = null;

    public const int DefaultFontSizeIndex = 7; // 0.12
    public const string DefaultTextHexColor = "#000000";

    public bool IsWindowOpen { get; private set; }
    public string TextHexColor { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        CloseWindow();

        TextHexColor = DefaultTextHexColor;
    }

    public void OpenWindow()
    {
        if (WhiteboardBehaviour.Instance == null)
        {
            Logger.LogError("Failed to open whiteboard editor window. Whiteboard instance was not found.");
            return;
        }

        if (Utils.IsQuickMenuOpen() || IsWindowOpen) return;

        HostOnlyObject.SetActive(NetworkUtils.IsServer);

        if (NetworkUtils.IsServer)
        {
            UpdateHostOnlyCheckbox();
        }

        IsWindowOpen = true;
        EditorWindowObject.SetActive(true);
        SetDataToUI(WhiteboardBehaviour.Instance.Data);
        Utils.SetCursorLockState(false);
        PlayerUtils.SetControlsEnabled(false);
    }

    public void CloseWindow()
    {
        if (ColorPickerBehaviour.Instance.IsWindowOpen)
        {
            ColorPickerBehaviour.Instance.CloseWindow();
        }

        IsWindowOpen = false;
        EditorWindowObject.SetActive(false);
        Utils.SetCursorLockState(true);
        PlayerUtils.SetControlsEnabled(true);
    }

    public void OnConfirmButtonClicked()
    {
        if (WhiteboardBehaviour.Instance == null)
        {
            Logger.LogError("Failed to confirm whiteboard changes. Whiteboard instance was not found.");
            return;
        }

        WhiteboardBehaviour.Instance.SetData(GetDataFromUI());

        CloseWindow();
    }

    public void OnCancelButtonClicked()
    {
        CloseWindow();
    }

    public void OnResetButtonClicked()
    {
        SetDataToUI(new WhiteboardData());
    }

    public void OnHostOnlyButtonClicked()
    {
        if (!NetworkUtils.IsServer) return;

        ConfigManager.Whiteboard_HostOnlyEdit.Value = !ConfigManager.Whiteboard_HostOnlyEdit.Value;
        UpdateHostOnlyCheckbox();
    }

    public void OnColorPickerButtonClicked()
    {
        if (ColorPickerBehaviour.Instance == null) return;

        ColorPickerBehaviour.Instance.OpenWindow();
    }

    private void UpdateHostOnlyCheckbox()
    {
        HostOnlyCheckedObject.SetActive(ConfigManager.Whiteboard_HostOnlyEdit.Value);
    }

    private WhiteboardData GetDataFromUI()
    {
        string displayText = DisplayTextInputField.text;
        string textHexColor = TextHexColor;
        int fontSizeIndex = FontSizeDropdown.value;
        int fontStyleIndex = FontStyleDropdown.value;
        int fontFamilyIndex = FontFamilyDropdown.value;
        int horizontalAlignmentIndex = HorizontalAlignmentDropdown.value;
        int verticalAlignmentIndex = VerticalAlignmentDropdown.value;

        return new WhiteboardData(displayText, textHexColor, fontSizeIndex, fontStyleIndex, fontFamilyIndex, horizontalAlignmentIndex, verticalAlignmentIndex);
    }

    private void SetDataToUI(WhiteboardData data)
    {
        if (data == null)
        {
            Logger.LogWarning("WhiteboardData is null in WhiteboardEditorBehaviour.SetDataToUI(); Setting WhiteboardData to default.");

            data = new WhiteboardData();
        }

        try
        {
            DisplayTextInputField.text = data.DisplayText;
            SetTextHexColor(data.TextHexColor);
            FontSizeDropdown.value = data.FontSizeIndex;
            FontStyleDropdown.value = data.FontStyleIndex;
            FontFamilyDropdown.value = data.FontFamilyIndex;
            HorizontalAlignmentDropdown.value = data.HorizontalAlignmentIndex;
            VerticalAlignmentDropdown.value = data.VerticalAlignmentIndex;
        }
        catch (System.Exception e)
        {
            Logger.LogError($"Failed to set whiteboard editor ui data.\n\n{e}");
        }
    }

    private void UpdateTextColorPreview()
    {
        if (ColorUtility.TryParseHtmlString(TextHexColor, out Color color))
        {
            TextColorPreviewImage.color = color;
        }
    }

    public void SetTextHexColor(string newTextColorHex)
    {
        TextHexColor = newTextColorHex;
        UpdateTextColorPreview();
    }
}
