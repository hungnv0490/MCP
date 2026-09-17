using System;
using UnityEngine;
using UnityEngine.UI;

public class E_BoxColorPopup : MonoBehaviour
{
    public static E_BoxColorPopup Instance { get; private set; }

    [SerializeField] private GameObject visual;
    [SerializeField] private Transform grid;
    [SerializeField] private Button toggleButton;
    [SerializeField] private Button closeButton;

    private Color? selectedColor;

    // Raised after a box is actually repainted, so listeners (e.g. the ball-container
    // preview spawner) can keep ball supply in sync with the new box colors without
    // this popup needing to know anything about containers.
    public event Action<Color, Color> BoxRecolored;

    private void Awake()
    {
        Instance = this;

        if (toggleButton != null)
            toggleButton.onClick.AddListener(Toggle);

        if (closeButton != null)
            closeButton.onClick.AddListener(Hide);

        foreach (Transform swatch in grid)
        {
            var button = swatch.GetComponent<Button>();
            var image = swatch.GetComponent<Image>();
            if (button == null || image == null)
                continue;

            Color color = image.color;
            button.onClick.AddListener(() => SelectColor(color));
        }

        // Clicking any other button in the scene dismisses this popup if it's open --
        // e.g. opening "Load Btn"/"Ball Count" or hitting a popup's close button
        // shouldn't leave the color picker hanging around. The toggle button and the
        // swatches inside grid keep their own behaviour (open/close, pick a color)
        // instead of also being forced closed here.
        foreach (Button button in FindObjectsByType<Button>(FindObjectsInactive.Include))
        {
            if (button == toggleButton || button.transform.IsChildOf(grid))
                continue;

            button.onClick.AddListener(HideIfShown);
        }
    }

    public void Toggle()
    {
        if (visual.activeSelf)
            Hide();
        else
            Show();
    }

    private void Show()
    {
        visual.SetActive(true);
    }

    private void Hide()
    {
        visual.SetActive(false);
        selectedColor = null;
    }

    private void HideIfShown()
    {
        if (visual.activeSelf)
            Hide();
    }

    public void SelectColor(Color color)
    {
        selectedColor = color;
    }

    public bool TryPaint(E_Box box)
    {
        if (!visual.activeSelf || selectedColor == null)
            return false;

        Color oldColor = box.CurrentColor;
        Color newColor = selectedColor.Value;

        box.SetColor(newColor);

        if (oldColor != newColor)
            BoxRecolored?.Invoke(oldColor, newColor);

        return true;
    }
}
