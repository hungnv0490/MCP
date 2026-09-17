using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Lets a designer redefine, per box color, how many ball containers exist and how
// many balls each holds -- e.g. red currently split as three containers [2,5,7]
// can be retyped as two containers "6,8". One row per color drawn (icon tinted to
// that color + an editable comma-separated count field); mirrors LoadPixcelsPopup's
// pool-spawned scroll-view row pattern.
public class E_BallCountPopup : MonoBehaviour
{
    public static E_BallCountPopup Instance { get; private set; }

    [SerializeField] private RectTransform content;
    [SerializeField] private Button closeButton;

    private readonly List<E_BallCountItem> spawnedItems = new();

    private void Awake()
    {
        Instance = this;

        if (closeButton != null)
            closeButton.onClick.AddListener(Hide);
    }

    public void Show()
    {
        gameObject.SetActive(true);
        Populate();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void Populate()
    {
        foreach (E_BallCountItem item in spawnedItems)
            GameObjectPool.Instance.ReturnToPool(item);
        spawnedItems.Clear();

        if (E_BoxesManager.Instance == null || E_BallContainerManager.Instance == null || GameObjectPool.Instance == null)
            return;

        Dictionary<Color, int> boxCounts = E_BoxesManager.Instance.GetColorCounts();
        Dictionary<Color, List<int>> containerCounts = E_BallContainerManager.Instance.GetContainerCountsByColor();

        foreach (KeyValuePair<Color, int> entry in boxCounts)
        {
            List<int> counts = containerCounts.TryGetValue(entry.Key, out List<int> existing)
                ? existing
                : new List<int> { entry.Value };

            E_BallCountItem item = GameObjectPool.Instance.Get<E_BallCountItem>(PoolType.E_BallCountItem, Vector3.zero, Quaternion.identity, content);
            if (item == null)
                continue;

            item.Setup(entry.Key, entry.Value, counts);
            spawnedItems.Add(item);
        }
    }
}
