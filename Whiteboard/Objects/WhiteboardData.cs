using com.github.zehsteam.Whiteboard.Managers;
using com.github.zehsteam.Whiteboard.MonoBehaviours;
using System;
using Unity.Netcode;

namespace com.github.zehsteam.Whiteboard.Objects;

[Serializable]
public class WhiteboardData : INetworkSerializable
{
    public string DisplayText;
    public string TextHexColor;
    public int FontSizeIndex;
    public int FontStyleIndex;
    public int FontFamilyIndex;
    public int HorizontalAlignmentIndex;
    public int VerticalAlignmentIndex;

    public WhiteboardData()
    {
        DisplayText = ConfigManager.Whiteboard_DefaultDisplayText?.Value;
        TextHexColor = WhiteboardEditor.DefaultTextHexColor;
        FontSizeIndex = WhiteboardEditor.DefaultFontSizeIndex;
    }

    public WhiteboardData(string displayText)
    {
        DisplayText = displayText;
        TextHexColor = WhiteboardEditor.DefaultTextHexColor;
        FontSizeIndex = WhiteboardEditor.DefaultFontSizeIndex;
    }

    public WhiteboardData(string displayText, string textHexColor, int fontSizeIndex, int fontStyleIndex, int fontFamilyIndex, int horizontalAlignmentIndex, int verticalAlignmentIndex) : this(displayText)
    {
        TextHexColor = textHexColor;
        FontSizeIndex = fontSizeIndex;
        FontStyleIndex = fontStyleIndex;
        FontFamilyIndex = fontFamilyIndex;
        HorizontalAlignmentIndex = horizontalAlignmentIndex;
        VerticalAlignmentIndex = verticalAlignmentIndex;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref DisplayText);
        serializer.SerializeValue(ref TextHexColor);
        serializer.SerializeValue(ref FontSizeIndex);
        serializer.SerializeValue(ref FontStyleIndex);
        serializer.SerializeValue(ref FontFamilyIndex);
        serializer.SerializeValue(ref HorizontalAlignmentIndex);
        serializer.SerializeValue(ref VerticalAlignmentIndex);
    }
}
