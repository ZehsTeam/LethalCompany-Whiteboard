using UnityEngine;

namespace com.github.zehsteam.Whiteboard.MonoBehaviours;

public class ColorPicker : MonoBehaviour
{
    public static ColorPicker Instance { get; private set; }

    [SerializeField]
    private GameObject _colorPickerWindowObject;

    [SerializeField]
    private ColorPickerControl _colorPickerControl;

    public bool IsWindowOpen {  get; private set; }

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
    }

    public void OpenWindow()
    {
        if (!WhiteboardEditor.Instance.IsWindowOpen || IsWindowOpen) return;

        IsWindowOpen = true;
        _colorPickerWindowObject.SetActive(true);
        _colorPickerControl.SetColor(WhiteboardEditor.Instance.TextHexColor);
    }

    public void CloseWindow()
    {
        IsWindowOpen = false;
        _colorPickerWindowObject.SetActive(false);
    }

    public void OnConfirmButtonClicked()
    {
        WhiteboardEditor.Instance.SetTextHexColor(_colorPickerControl.GetHexColor());

        CloseWindow();
    }

    public void OnCancelButtonClicked()
    {
        CloseWindow();
    }
}
