using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace com.github.zehsteam.Whiteboard.MonoBehaviours;

public class ColorPickerControl : MonoBehaviour
{
    [SerializeField]
    private float _currentHue;

    [SerializeField]
    private float _currentSat;

    [SerializeField]
    private float _currentVal;

    [SerializeField]
    private RawImage _hueImage;

    [SerializeField]
    private RawImage _satValImage;

    [SerializeField]
    private RawImage _outputImage;

    [SerializeField]
    private Slider _hueSlider = null;

    [SerializeField]
    private TMP_InputField _hexColorInputField = null;

    [SerializeField]
    private SVImageControl _svImageControlBehaviour;

    private Texture2D _hueTexture;
    private Texture2D _satValTexture;
    private Texture2D _outputTexture;

    private bool _updatedHexColorInputFieldInternally;
    private bool _initialized;

    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        if (_initialized) return;
        _initialized = true;

        CreateHueImage();
        CreateSatValImage();
        CreateOutputImage();

        UpdateOutputImage();
    }

    private void CreateHueImage()
    {
        _hueTexture = new Texture2D(1, 16);
        _hueTexture.wrapMode = TextureWrapMode.Clamp;
        _hueTexture.name = "HueTexture";

        for (int i = 0; i < _hueTexture.height; i++)
        {
            _hueTexture.SetPixel(0, i, Color.HSVToRGB(i / (float)_hueTexture.height, 1, 0.95f));
        }

        _hueTexture.Apply();
        _currentHue = 0f;

        _hueImage.texture = _hueTexture;
    }

    private void CreateSatValImage()
    {
        _satValTexture = new Texture2D(16, 16);
        _satValTexture.wrapMode = TextureWrapMode.Clamp;
        _satValTexture.name = "SatValTexture";

        for (int y = 0; y < _satValTexture.height; y++)
        {
            for (int x = 0; x < _satValTexture.width; x++)
            {
                _satValTexture.SetPixel(x, y, Color.HSVToRGB(_currentHue, x / (float)_satValTexture.width, y / (float)_satValTexture.height));
            }
        }

        _satValTexture.Apply();
        _currentSat = 0f;
        _currentVal = 0f;

        _satValImage.texture = _satValTexture;
    }

    private void CreateOutputImage()
    {
        _outputTexture = new Texture2D(1, 16);
        _outputTexture.wrapMode = TextureWrapMode.Clamp;
        _outputTexture.name = "OutputTexture";

        Color currentColor = Color.HSVToRGB(_currentHue, _currentSat, _currentVal);

        for (int i = 0; i < _outputTexture.height; i++)
        {
            _outputTexture.SetPixel(0, i, currentColor);
        }

        _outputTexture.Apply();
        _outputImage.texture = _outputTexture;
    }

    private void UpdateOutputImage()
    {
        Color currentColor = Color.HSVToRGB(_currentHue, _currentSat, _currentVal);

        for (int i = 0; i < _outputTexture.height; i++)
        {
            _outputTexture.SetPixel(0, i, currentColor);
        }

        _outputTexture.Apply();

        _updatedHexColorInputFieldInternally = true;
        _hexColorInputField.text = GetHexColor();
    }

    public void SetSatVal(float saturation, float value)
    {
        _currentSat = saturation;
        _currentVal = value;

        UpdateOutputImage();
    }

    public void UpdateSatValImage()
    {
        _currentHue = _hueSlider.value;

        for (int y = 0; y < _satValTexture.height; y++)
        {
            for (int x = 0; x < _satValTexture.width; x++)
            {
                _satValTexture.SetPixel(x, y, Color.HSVToRGB(_currentHue, x / (float)_satValTexture.width, y / (float)_satValTexture.height));
            }
        }

        _satValTexture.Apply();

        UpdateOutputImage();
    }

    public void OnHexColorInputFieldValueChanged()
    {
        if (_updatedHexColorInputFieldInternally)
        {
            _updatedHexColorInputFieldInternally = false;
            return;
        }

        if (_hexColorInputField.text.Length < 6) return;

        string hexColor;

        if (_hexColorInputField.text.StartsWith("#"))
        {
            hexColor = _hexColorInputField.text;
        }
        else
        {
            hexColor = $"#{_hexColorInputField.text}";
        }

        UpdateColor(hexColor);
    }

    private void UpdateColor(string hexColor)
    {
        if (ColorUtility.TryParseHtmlString(hexColor, out Color newColor))
        {
            Color.RGBToHSV(newColor, out _currentHue, out _currentSat, out _currentVal);

            _hueSlider.value = _currentHue;

            UpdateOutputImage();

            _svImageControlBehaviour.SetPickerLocation(_currentSat, _currentVal);
        }
    }

    public void SetColor(string hexColor)
    {
        Initialize();

        _updatedHexColorInputFieldInternally = true;
        _hexColorInputField.text = hexColor;

        UpdateColor(hexColor);
    }

    public string GetHexColor()
    {
        Color currentColor = Color.HSVToRGB(_currentHue, _currentSat, _currentVal);
        return $"#{ColorUtility.ToHtmlStringRGB(currentColor)}";
    }
}
