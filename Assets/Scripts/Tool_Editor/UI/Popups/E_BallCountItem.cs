using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// One row in E_BallCountPopup's scroll view: a color swatch plus a comma-separated
// text field the designer edits to redefine how that color's balls are split into
// containers, e.g. "6,8" -> two containers of 6 and 8. Applied on end-edit; an
// invalid entry (wrong total, non-positive value, empty) is rejected and the field
// snaps back to the last valid text, per E_BallContainerSpawner.TryApplyColorEdit.
public class E_BallCountItem : MonoBehaviour, IPoolable
{
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text totalLabel;
    [SerializeField] private TMP_InputField input;

    private Color colorValue;
    private string lastValidText;

    private void Awake()
    {
        input.onEndEdit.AddListener(HandleEndEdit);
    }

    // totalBalls is the color's fixed box total (the sum the comma list must match) --
    // shown above the icon so the designer knows what to split, independent of
    // however the current containers happen to be divided up.
    public void Setup(Color color, int totalBalls, List<int> counts)
    {
        colorValue = color;
        icon.color = color;
        totalLabel.text = totalBalls.ToString();
        lastValidText = string.Join(",", counts);
        input.text = lastValidText;
    }

    private void HandleEndEdit(string text)
    {
        if (TryParseAndApply(text))
            lastValidText = text;
        else
            input.text = lastValidText;
    }

    private bool TryParseAndApply(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return false;

        string[] parts = text.Split(',');
        List<int> counts = new(parts.Length);

        foreach (string part in parts)
        {
            if (!int.TryParse(part.Trim(), out int value) || value <= 0)
                return false;

            counts.Add(value);
        }

        return E_BallContainerSpawner.Instance != null && E_BallContainerSpawner.Instance.TryApplyColorEdit(colorValue, counts);
    }

    public void OnSpawn()
    {
    }

    public void OnDespawn()
    {
        input.text = string.Empty;
    }
}
