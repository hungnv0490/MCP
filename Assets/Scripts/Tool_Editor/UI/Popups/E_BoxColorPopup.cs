using UnityEngine;
using UnityEngine.UI;

public class E_BoxColorPopup : MonoBehaviour
{
    public static E_BoxColorPopup Instance { get; private set; }

    [SerializeField] private GameObject visual;
    [SerializeField] private Transform grid;
    [SerializeField] private Button toggleButton;

    private Color? selectedColor;

    private void Awake()
    {
        Instance = this;

        if (toggleButton != null)
            toggleButton.onClick.AddListener(Toggle);

        foreach (Transform swatch in grid)
        {
            var button = swatch.GetComponent<Button>();
            var image = swatch.GetComponent<Image>();
            if (button == null || image == null)
                continue;

            Color color = image.color;
            button.onClick.AddListener(() => SelectColor(color));
        }
    }

    public void Toggle()
    {
        bool show = !visual.activeSelf;
        visual.SetActive(show);

        if (!show)
            selectedColor = null;
    }

    public void SelectColor(Color color)
    {
        selectedColor = color;
    }

    public bool TryPaint(E_Box box)
    {
        if (!visual.activeSelf || selectedColor == null)
            return false;

        box.SetColor(selectedColor.Value);
        return true;
    }
}
