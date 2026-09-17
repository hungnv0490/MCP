using System.Collections.Generic;
using UnityEngine;

// Regenerates the ball-container difficulty preview every time E_BoxesManager
// finishes drawing boxes from a pixel image. Splits each drawn color's total box
// count into a random number of containers of at least 2 balls each (summing back
// to that exact total), shuffles them, and deals them round-robin into a fixed
// number of columns -- some of which may end up empty if there isn't enough
// content to fill all of them -- so a designer can see and hand-tune ball supply
// vs. box demand.
public class E_BallContainerSpawner : MonoBehaviour
{
    public static E_BallContainerSpawner Instance { get; private set; }

    [SerializeField] private E_ContainerSlot slotPrefab;
    [SerializeField] private Transform origin;

    [Tooltip("Always creates this many column slots. Ones that don't end up with any " +
             "container (not enough content to fill them) stay empty -- later, loading a " +
             "level in the Game scene will treat an empty column as one that isn't spawned.")]
    [SerializeField] private int columnCount = 6;

    [Tooltip("Every container holds at least 2 balls (never a lone 1), so splitting a " +
             "color's box total picks this many random-sized containers when possible.")]
    [SerializeField] private Vector2Int containerCountRangePerColor = new(1, 4);
    [SerializeField] private float columnSpacing = 1.2f;
    [SerializeField] private float rowSpacing = 1.2f;

    private readonly List<E_BallContainer> _activeContainers = new();
    private readonly List<E_ContainerSlot> _activeSlots = new();

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // Subscribe in Start rather than OnEnable -- Awake/OnEnable order across
        // different GameObjects isn't guaranteed, so E_BoxesManager.Instance can
        // still be null when this object's OnEnable runs. By Start, every other
        // object's Awake has already run.
        if (E_BoxesManager.Instance != null)
            E_BoxesManager.Instance.BoxesDrawn += GenerateContainers;

        if (E_BoxColorPopup.Instance != null)
            E_BoxColorPopup.Instance.BoxRecolored += HandleBoxRecolored;
    }

    private void OnDisable()
    {
        if (E_BoxesManager.Instance != null)
            E_BoxesManager.Instance.BoxesDrawn -= GenerateContainers;

        if (E_BoxColorPopup.Instance != null)
            E_BoxColorPopup.Instance.BoxRecolored -= HandleBoxRecolored;
    }

    // A single box changed color: the old color now has one less box demanding
    // balls and the new color has one more, so nudge one existing container of
    // each by -+1 instead of a full regenerate (which would reshuffle every
    // column the designer may have already hand-tuned).
    private void HandleBoxRecolored(Color oldColor, Color newColor)
    {
        if (GameObjectPool.Instance == null)
            return;

        DecrementColor(oldColor);
        IncrementColor(newColor);
    }

    private void DecrementColor(Color color)
    {
        E_BallContainer container = E_BallContainerManager.Instance.GetFirstContainerOfColor(color);
        if (container == null)
            return;

        int newCount = container.BallCount - 1;
        if (newCount <= 0)
        {
            E_BallContainerManager.Instance.RemoveContainer(container);
            _activeContainers.Remove(container);
            GameObjectPool.Instance.ReturnToPool(container);
        }
        else
        {
            container.SetBallCount(newCount);
        }
    }

    private void IncrementColor(Color color)
    {
        E_BallContainer container = E_BallContainerManager.Instance.GetFirstContainerOfColor(color);
        if (container != null)
        {
            container.SetBallCount(container.BallCount + 1);
            return;
        }

        // No container exists yet for this color (e.g. it's brand new to the box
        // grid) -- create a fresh one instead of silently losing that ball.
        container = GameObjectPool.Instance.Get<E_BallContainer>(PoolType.E_BallContainer, Vector3.zero, Quaternion.identity, transform);
        if (container == null)
            return;

        container.Setup(color, 1, -1);
        _activeContainers.Add(container);
        E_BallContainerManager.Instance.AddContainerToLeastFullColumn(container);
    }

    private void GenerateContainers()
    {
        ClearContainers();

        if (GameObjectPool.Instance == null || origin == null)
            return;

        List<(Color color, int count)> specs = BuildContainerSpecs(E_BoxesManager.Instance.GetColorCounts());
        if (specs.Count == 0)
            return;

        Shuffle(specs);

        int columns = Mathf.Max(1, columnCount);
        float startX = -(columns - 1) * columnSpacing * 0.5f;

        List<List<(Color color, int count)>> perColumn = new();
        for (int col = 0; col < columns; col++)
            perColumn.Add(new List<(Color, int)>());

        for (int i = 0; i < specs.Count; i++)
            perColumn[i % columns].Add(specs[i]);

        for (int col = 0; col < columns; col++)
            SpawnColumn(col, origin.position + new Vector3(startX + col * columnSpacing, 0f, 0f), perColumn[col]);
    }

    private void SpawnColumn(int columnIndex, Vector3 columnAnchor, List<(Color color, int count)> specs)
    {
        List<E_BallContainer> containers = new();

        for (int row = 0; row < specs.Count; row++)
        {
            Vector3 position = columnAnchor + new Vector3(0f, -row * rowSpacing, 0f);
            E_BallContainer container = GameObjectPool.Instance.Get<E_BallContainer>(PoolType.E_BallContainer, position, Quaternion.identity, transform);
            if (container == null)
                continue;

            container.Setup(specs[row].color, specs[row].count, columnIndex);
            containers.Add(container);
            _activeContainers.Add(container);
        }

        Vector3 slotPosition = columnAnchor + new Vector3(0f, -containers.Count * rowSpacing, 0f);
        E_ContainerSlot slot = Instantiate(slotPrefab, slotPosition, Quaternion.identity, transform);
        _activeSlots.Add(slot);

        E_BallContainerManager.Instance.RegisterColumn(columnIndex, columnAnchor, containers, slot);
    }

    // Replaces every container of the given color with a new set of containers
    // whose ball counts are exactly newCounts, e.g. red currently [2,5,7] ->
    // newCounts [6,8]. Rejects the edit entirely (leaving that color untouched) if
    // the counts don't sum back to that color's exact box total or contain a
    // non-positive value -- used by E_BallCountPopup so a mistyped edit can't
    // break the ball-supply/box-demand match.
    public bool TryApplyColorEdit(Color color, List<int> newCounts)
    {
        if (newCounts == null || newCounts.Count == 0 || GameObjectPool.Instance == null)
            return false;

        int expectedTotal = E_BoxesManager.Instance.GetColorCounts().TryGetValue(color, out int total) ? total : 0;

        int enteredTotal = 0;
        foreach (int count in newCounts)
        {
            if (count <= 0)
                return false;

            enteredTotal += count;
        }

        if (enteredTotal != expectedTotal)
            return false;

        List<E_BallContainer> removed = E_BallContainerManager.Instance.RemoveContainersOfColor(color);
        foreach (E_BallContainer container in removed)
        {
            _activeContainers.Remove(container);
            GameObjectPool.Instance.ReturnToPool(container);
        }

        foreach (int count in newCounts)
        {
            E_BallContainer container = GameObjectPool.Instance.Get<E_BallContainer>(PoolType.E_BallContainer, Vector3.zero, Quaternion.identity, transform);
            if (container == null)
                continue;

            container.Setup(color, count, -1);
            _activeContainers.Add(container);
            E_BallContainerManager.Instance.AddContainerToLeastFullColumn(container);
        }

        return true;
    }

    private void ClearContainers()
    {
        if (E_BallContainerManager.Instance != null)
            E_BallContainerManager.Instance.ClearAll();

        foreach (E_BallContainer container in _activeContainers)
            GameObjectPool.Instance.ReturnToPool(container);
        _activeContainers.Clear();

        foreach (E_ContainerSlot slot in _activeSlots)
        {
            if (slot != null)
                Destroy(slot.gameObject);
        }
        _activeSlots.Clear();
    }

    // For each color, splits its total box count into a random number of positive
    // parts (within containerCountRangePerColor) that sum back to that exact total.
    private List<(Color color, int count)> BuildContainerSpecs(Dictionary<Color, int> colorCounts)
    {
        List<(Color color, int count)> specs = new();

        foreach (KeyValuePair<Color, int> entry in colorCounts)
        {
            foreach (int part in SplitCount(entry.Value, containerCountRangePerColor.x, containerCountRangePerColor.y))
                specs.Add((entry.Key, part));
        }

        return specs;
    }

    // Every container must hold at least 2 balls, so at most total/2 containers can
    // be made from a given total (e.g. total=5 -> at most 2 containers of >=2 each).
    // total=1 can't satisfy that at all -- falls back to a single container of 1
    // rather than dropping that color's balls entirely (the box/ball total must match).
    private static List<int> SplitCount(int total, int minParts, int maxParts)
    {
        if (total < 2)
            return new List<int> { total };

        minParts = Mathf.Max(1, minParts);
        maxParts = Mathf.Max(minParts, maxParts);
        int parts = Mathf.Min(Random.Range(minParts, maxParts + 1), total / 2);

        List<int> counts = new(parts);
        for (int i = 0; i < parts; i++)
            counts.Add(2);

        int remaining = total - parts * 2;
        for (int i = 0; i < remaining; i++)
            counts[Random.Range(0, parts)]++;

        return counts;
    }

    private static void Shuffle<T>(IList<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
