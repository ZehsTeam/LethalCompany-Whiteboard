using com.github.zehsteam.Whiteboard.Helpers;
using com.github.zehsteam.Whiteboard.Managers;
using com.github.zehsteam.Whiteboard.Objects;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace com.github.zehsteam.Whiteboard.MonoBehaviours;

public class WhiteboardEditor : MonoBehaviour
{
    public static WhiteboardEditor Instance { get; private set; }

    [SerializeField]
    private GameObject _editorWindowObject;

    [SerializeField]
    private TMP_InputField _displayTextInputField;

    [SerializeField]
    private GameObject _hostOnlyObject;

    [SerializeField]
    private Button _hostOnlyButton;

    [SerializeField]
    private GameObject _hostOnlyCheckedObject;

    [SerializeField]
    private Image _textColorPreviewImage;

    [SerializeField]
    private TMP_Dropdown _fontSizeDropdown;

    [SerializeField]
    private TMP_Dropdown _fontStyleDropdown;

    [SerializeField]
    private TMP_Dropdown _fontFamilyDropdown;

    [SerializeField]
    private TMP_Dropdown _horizontalAlignmentDropdown;

    [SerializeField]
    private TMP_Dropdown _verticalAlignmentDropdown;

    public const int DefaultFontSizeIndex = 7; // 0.12
    public const string DefaultTextHexColor = "#000000";

    public bool IsWindowOpen { get; private set; }
    public string TextHexColor { get; private set; }

    public static void Spawn()
    {
        if (Instance != null)
            return;

        Instantiate(Assets.WhiteboardEditorCanvasPrefab);

        Logger.LogInfo("Spawned WhiteboardEditorCanvas.");
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
    }

    private void Start()
    {
        CloseWindow();

        TextHexColor = DefaultTextHexColor;

        InitializeFontSizeDropdown();
        InitializeFontStyleDropdown();
    }

    private void InitializeFontSizeDropdown()
    {
        _fontSizeDropdown.ClearOptions();

        List<TMP_Dropdown.OptionData> options = [];

        int length = Whiteboard.FontSizeArray.Length;
        int baseValue = 5;

        for (int i = 0; i < length; i++)
        {
            options.Add(new($"{baseValue + i}"));
        }

        _fontSizeDropdown.AddOptions(options);
    }

    private void InitializeFontStyleDropdown()
    {
        _fontStyleDropdown.ClearOptions();

        List<TMP_Dropdown.OptionData> options = [];

        foreach (var item in Whiteboard.FontStyleArray)
        {
            options.Add(new(item.ToString()));
        }

        _fontStyleDropdown.AddOptions(options);
    }

    public void OpenWindow()
    {
        if (Whiteboard.Instance == null)
        {
            Logger.LogError("Failed to open whiteboard editor window. Whiteboard instance was not found.");
            return;
        }

        if (PlayerUtils.IsQuickMenuOpen() || IsWindowOpen)
            return;

        _hostOnlyObject.SetActive(NetworkUtils.IsServer);

        if (NetworkUtils.IsServer)
        {
            UpdateHostOnlyCheckbox();
        }

        IsWindowOpen = true;
        _editorWindowObject.SetActive(true);
        SetUIFromData(Whiteboard.Instance.Data);
        PlayerUtils.SetCursorLockState(false);
        PlayerUtils.SetControlsEnabled(false);
    }

    public void CloseWindow()
    {
        if (ColorPicker.Instance.IsWindowOpen)
        {
            ColorPicker.Instance.CloseWindow();
        }

        IsWindowOpen = false;
        _editorWindowObject.SetActive(false);
        PlayerUtils.SetCursorLockState(true);
        PlayerUtils.SetControlsEnabled(true);
    }

    public void OnConfirmButtonClicked()
    {
        if (Whiteboard.Instance == null)
        {
            Logger.LogError("Failed to confirm whiteboard changes. Whiteboard instance was not found.");
            return;
        }

        Whiteboard.Instance.SetData(GetDataFromUI());

        CloseWindow();
    }

    public void OnCancelButtonClicked()
    {
        CloseWindow();
    }

    public void OnResetButtonClicked()
    {
        SetUIFromData(new WhiteboardData());
    }

    public void OnHostOnlyButtonClicked()
    {
        if (!NetworkUtils.IsServer) return;

        ConfigManager.Whiteboard_HostOnlyEdit.Value = !ConfigManager.Whiteboard_HostOnlyEdit.Value;
        UpdateHostOnlyCheckbox();
    }

    public void OnColorPickerButtonClicked()
    {
        if (ColorPicker.Instance == null) return;

        ColorPicker.Instance.OpenWindow();
    }

    private void UpdateHostOnlyCheckbox()
    {
        _hostOnlyCheckedObject.SetActive(ConfigManager.Whiteboard_HostOnlyEdit.Value);
    }

    private WhiteboardData GetDataFromUI()
    {
        string displayText = _displayTextInputField.text;
        string textHexColor = TextHexColor;
        int fontSizeIndex = _fontSizeDropdown.value;
        int fontStyleIndex = _fontStyleDropdown.value;
        int fontFamilyIndex = _fontFamilyDropdown.value;
        int horizontalAlignmentIndex = _horizontalAlignmentDropdown.value;
        int verticalAlignmentIndex = _verticalAlignmentDropdown.value;

        return new WhiteboardData(displayText, textHexColor, fontSizeIndex, fontStyleIndex, fontFamilyIndex, horizontalAlignmentIndex, verticalAlignmentIndex);
    }

    private void SetUIFromData(WhiteboardData data)
    {
        if (data == null)
        {
            Logger.LogWarning($"{nameof(WhiteboardData)} is null in {nameof(WhiteboardEditor)}.{nameof(SetUIFromData)}() Setting {nameof(WhiteboardData)} to default.");

            data = new WhiteboardData();
        }

        try
        {
            _displayTextInputField.text = data.DisplayText;
            SetTextHexColor(data.TextHexColor);
            _fontSizeDropdown.value = data.FontSizeIndex;
            _fontStyleDropdown.value = data.FontStyleIndex;
            _fontFamilyDropdown.value = data.FontFamilyIndex;
            _horizontalAlignmentDropdown.value = data.HorizontalAlignmentIndex;
            _verticalAlignmentDropdown.value = data.VerticalAlignmentIndex;
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to set whiteboard editor ui data. {ex}");
        }
    }

    private void UpdateTextColorPreview()
    {
        if (ColorUtility.TryParseHtmlString(TextHexColor, out Color color))
        {
            _textColorPreviewImage.color = color;
        }
    }

    public void SetTextHexColor(string newTextColorHex)
    {
        TextHexColor = newTextColorHex;
        UpdateTextColorPreview();
    }
}
