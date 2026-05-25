using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace com.github.zehsteam.Whiteboard.MonoBehaviours;

public class SVImageControl : MonoBehaviour, IDragHandler, IPointerClickHandler
{
    [SerializeField]
    private ColorPickerControl _colorPickerControlBehaviour;

    [SerializeField]
    private Image _pickerImage;

    private RectTransform _rectTransform, _pickerTransform;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();

        _pickerTransform = _pickerImage.GetComponent<RectTransform>();
        _pickerTransform.localPosition = new Vector2(-(_rectTransform.sizeDelta.x - 0.5f), -(_rectTransform.sizeDelta.y - 0.5f));
    }

    private void UpdateColor(PointerEventData eventData)
    {
        Vector3 position = _rectTransform.InverseTransformPoint(eventData.position);

        float deltaX = _rectTransform.sizeDelta.x - 0.5f;
        float deltaY = _rectTransform.sizeDelta.y - 0.5f;

        position.x = Mathf.Clamp(position.x, -deltaX, deltaX);
        position.y = Mathf.Clamp(position.y, -deltaY, deltaY);

        float x = position.x + deltaX;
        float y = position.y + deltaY;

        float xNorm = x / _rectTransform.sizeDelta.x;
        float yNorm = y / _rectTransform.sizeDelta.y;

        _pickerTransform.localPosition = position;
        _pickerImage.color = Color.HSVToRGB(0, 0, 1 - yNorm);

        _colorPickerControlBehaviour.SetSatVal(xNorm, yNorm);
    }

    public void OnDrag(PointerEventData eventData)
    {
        UpdateColor(eventData);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        UpdateColor(eventData);
    }

    public void SetPickerLocation(float saturation, float value)
    {
        float x = saturation - _rectTransform.sizeDelta.x - (_rectTransform.sizeDelta.x - 0.5f);
        float y = value - _rectTransform.sizeDelta.y - (_rectTransform.sizeDelta.y - 0.5f);

        _pickerTransform.localPosition = new Vector2(x, y);
        _pickerImage.color = Color.HSVToRGB(0, 0, 1 - value);

        Logger.LogInfo($"SetPickerLocation (saturation: {saturation}, value: {value}), (x: {x}, y: {y})", extended: true);
    }
}
