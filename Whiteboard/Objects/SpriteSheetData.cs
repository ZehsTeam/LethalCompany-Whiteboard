using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace com.github.zehsteam.Whiteboard.Objects;

[CreateAssetMenu(menuName = "Whiteboard/SpriteSheetData")]
public class SpriteSheetData : ScriptableObject
{
    [Header("Editor Buttons")]
    [Space(5f)]
    public bool ImportSpriteData = false;

    [Header("Data")]
    [Space(5f)]
    public TMP_SpriteAsset SpriteAsset;

    [TextArea(3, 20)]
    public string SpriteDataImportCode;

    public List<SpriteSheetItem> SpriteData = [];

    public string GetAllSpritesText()
    {
        string text = string.Empty;

        foreach (var item in SpriteData)
        {
            text += $"{item.GetText()} ";
        }

        return text.Trim();
    }

    public string GetParsedText(string text, bool matchCase = false)
    {
        string result = text;

        System.StringComparison stringComparison = matchCase ? System.StringComparison.Ordinal : System.StringComparison.OrdinalIgnoreCase;

        if (result.Contains("<all>", stringComparison))
        {
            result = result.Replace("<all>", GetAllSpritesText(), stringComparison);
        }

        List<SpriteSheetItem> orderedSpriteData = SpriteData.OrderBy(_ => _.Name.Length).Reverse().ToList();

        foreach (var item in orderedSpriteData)
        {
            if (result.Contains(item.Name, stringComparison))
            {
                result = result.Replace(item.Name, item.GetText(), stringComparison);
            }
        }

        return result;
    }

    #region Editor
    private void OnValidate()
    {
        if (ImportSpriteData)
        {
            ImportSpriteData = false;
            OnImportSpriteDataClicked();
        }
    }

    private void OnImportSpriteDataClicked()
    {
        if (string.IsNullOrWhiteSpace(SpriteDataImportCode))
        {
            LogError("Failed to import sprite data code. Sprite data import code is null or empty.");
            return;
        }

        string[] entries = SpriteDataImportCode.Trim().Split(",", System.StringSplitOptions.RemoveEmptyEntries);

        if (entries.Length == 0)
        {
            LogError("Failed to import sprite data code. Sprite data import code contains no entries.");
            return;
        }

        SpriteData = [];

        for (int entryIndex = 0; entryIndex < entries.Length; entryIndex++)
        {
            if (string.IsNullOrWhiteSpace(entries[entryIndex]))
            {
                LogEntryError(entryIndex, $"Entry is null or empty.");
                continue;
            }

            string[] items = entries[entryIndex].Trim().Split(":", System.StringSplitOptions.RemoveEmptyEntries);

            if (items.Length < 3)
            {
                LogEntryError(entryIndex, $"Entry has less than 3 items.");
                continue;
            }

            string name = items[0];

            if (!TryParseInt(entryIndex, items[1], out int index)) continue;
            if (!TryParseInt(entryIndex, items[2], out int endIndex)) continue;

            float animationSpeed = 10f;

            if (items.Length >= 4 && !TryParseFloat(entryIndex, items[3], out animationSpeed))
            {
                continue;
            }

            SpriteData.Add(new SpriteSheetItem(name, index, endIndex, animationSpeed));
        }

        LogInfo("Finished importing sprite data from sprite data import code.");
    }

    private static bool TryParseInt(int entryIndex, string text, out int result)
    {
        if (!int.TryParse(text, out result))
        {
            LogEntryError(entryIndex, $"Could not parse \"{text}\" as an integer.");
            return false;
        }

        return true;
    }

    private static bool TryParseFloat(int entryIndex, string text, out float result)
    {
        if (!float.TryParse(text, out result))
        {
            LogEntryError(entryIndex, $"Could not parse \"{text}\" as a float.");
            return false;
        }

        return true;
    }

    private static void LogEntryError(int entryIndex, object data)
    {
        LogError($"Failed to import sprite data entry #{entryIndex}. " + data);
    }

    private static void LogInfo(object data)
    {
        Debug.Log($"[SpriteSheetData] " + data);
    }

    private static void LogError(object data)
    {
        Debug.LogError($"[SpriteSheetData] " + data);
    }
    #endregion
}

[System.Serializable]
public class SpriteSheetItem
{
    public string Name;
    public int Index;
    public int EndIndex;
    public float AnimationSpeed;

    public SpriteSheetItem(string name, int index, int endIndex, float animationSpeed)
    {
        Name = name;
        Index = index;
        EndIndex = endIndex;
        AnimationSpeed = animationSpeed;
    }

    public string GetText()
    {
        if (EndIndex > Index)
        {
            return $"<sprite anim=\"{Index},{EndIndex},{AnimationSpeed}\">";
        }

        return $"<sprite={Index}>";
    }
}
