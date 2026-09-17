using System.Collections.Generic;
using UnityEngine;

// Owns the column layout for the ball-container difficulty preview and handles
// dragging a container from one column into an empty slot in another. Mirrors
// BallContainerManager's column bookkeeping/shift-up logic, but reordering
// (drag-drop) replaces launch-slot activation as the only interaction.
public class E_BallContainerManager : MonoBehaviour
{
    public static E_BallContainerManager Instance { get; private set; }

    [SerializeField] private float rowSpacing = 1.2f;

    private readonly Dictionary<int, List<E_BallContainer>> _columns = new();
    private readonly Dictionary<int, E_ContainerSlot> _slots = new();

    // Row-0 (topmost container) world position for each column -- fixed for the
    // lifetime of a generation, independent of how many containers currently
    // occupy the column, so a column can be repositioned even after emptying out.
    private readonly Dictionary<int, Vector3> _columnAnchors = new();

    private void Awake()
    {
        Instance = this;
    }

    public void ClearAll()
    {
        _columns.Clear();
        _slots.Clear();
        _columnAnchors.Clear();
    }

    public void RegisterColumn(int columnIndex, Vector3 columnAnchor, List<E_BallContainer> containers, E_ContainerSlot slot)
    {
        _columnAnchors[columnIndex] = columnAnchor;
        _columns[columnIndex] = containers;
        _slots[columnIndex] = slot;
        slot.ColumnIndex = columnIndex;

        foreach (E_BallContainer container in containers)
            container.SetColumnIndex(columnIndex);

        RepositionColumn(columnIndex);
    }

    public void TryDrop(E_BallContainer dragged, Vector3 dropWorldPosition, Vector3 originalPosition)
    {
        E_ContainerSlot targetSlot = FindSlotAt(dropWorldPosition);

        if (targetSlot == null || targetSlot.ColumnIndex == dragged.ColumnIndex)
        {
            dragged.MoveTo(originalPosition);
            return;
        }

        int sourceColumnIndex = dragged.ColumnIndex;
        int targetColumnIndex = targetSlot.ColumnIndex;

        _columns[sourceColumnIndex].Remove(dragged);
        RepositionColumn(sourceColumnIndex);

        dragged.SetColumnIndex(targetColumnIndex);
        _columns[targetColumnIndex].Add(dragged);
        RepositionColumn(targetColumnIndex);
    }

    // Removes every container of the given color from wherever it currently sits
    // (regardless of column), repositions every affected column to close the gaps,
    // and hands the removed containers back to the caller (E_BallContainerSpawner)
    // to return to the pool.
    public List<E_BallContainer> RemoveContainersOfColor(Color color)
    {
        List<E_BallContainer> removed = new();

        foreach (KeyValuePair<int, List<E_BallContainer>> pair in _columns)
        {
            List<E_BallContainer> column = pair.Value;

            for (int i = column.Count - 1; i >= 0; i--)
            {
                if (column[i].ColorValue == color)
                {
                    removed.Add(column[i]);
                    column.RemoveAt(i);
                }
            }
        }

        if (removed.Count > 0)
        {
            foreach (int columnIndex in _columns.Keys)
                RepositionColumn(columnIndex);
        }

        return removed;
    }

    // Appends a freshly-created container to whichever column currently holds the
    // fewest containers, so replacing one color's containers doesn't pile them all
    // into a single column.
    public void AddContainerToLeastFullColumn(E_BallContainer container)
    {
        int bestColumn = -1;
        int bestCount = int.MaxValue;

        foreach (KeyValuePair<int, List<E_BallContainer>> pair in _columns)
        {
            if (pair.Value.Count < bestCount)
            {
                bestCount = pair.Value.Count;
                bestColumn = pair.Key;
            }
        }

        if (bestColumn == -1)
            return;

        container.SetColumnIndex(bestColumn);
        _columns[bestColumn].Add(container);
        RepositionColumn(bestColumn);
    }

    // First container found of the given color, regardless of column -- used to pick
    // a target for a +-1 ball-count nudge when a single box gets repainted (as
    // opposed to RemoveContainersOfColor's full-color replace).
    public E_BallContainer GetFirstContainerOfColor(Color color)
    {
        foreach (List<E_BallContainer> column in _columns.Values)
        {
            foreach (E_BallContainer container in column)
            {
                if (container.ColorValue == color)
                    return container;
            }
        }

        return null;
    }

    // Removes one specific container (wherever it is) and closes the gap in its
    // column -- unlike RemoveContainersOfColor, this targets a single instance.
    public void RemoveContainer(E_BallContainer container)
    {
        foreach (KeyValuePair<int, List<E_BallContainer>> pair in _columns)
        {
            if (pair.Value.Remove(container))
            {
                RepositionColumn(pair.Key);
                return;
            }
        }
    }

    // Current ball-count breakdown per color, e.g. red -> [2, 5, 7], used to
    // pre-fill E_BallCountPopup's rows with the currently active distribution.
    public Dictionary<Color, List<int>> GetContainerCountsByColor()
    {
        Dictionary<Color, List<int>> result = new();

        foreach (List<E_BallContainer> column in _columns.Values)
        {
            foreach (E_BallContainer container in column)
            {
                if (!result.TryGetValue(container.ColorValue, out List<int> counts))
                {
                    counts = new List<int>();
                    result[container.ColorValue] = counts;
                }

                counts.Add(container.BallCount);
            }
        }

        return result;
    }

    // Vertical extent of the current column layout (top of the columns down to the
    // lowest empty slot, i.e. the bottom of the tallest column), used by
    // E_CameraPanController to know how far it's allowed to pan.
    public bool TryGetColumnsBoundsY(out float minY, out float maxY)
    {
        minY = float.MaxValue;
        maxY = float.MinValue;
        bool any = false;

        foreach (Vector3 anchor in _columnAnchors.Values)
        {
            maxY = Mathf.Max(maxY, anchor.y);
            any = true;
        }

        foreach (E_ContainerSlot slot in _slots.Values)
        {
            if (slot == null)
                continue;

            minY = Mathf.Min(minY, slot.transform.position.y);
            any = true;
        }

        return any;
    }

    private static E_ContainerSlot FindSlotAt(Vector3 worldPosition)
    {
        Collider2D[] hits = Physics2D.OverlapPointAll(worldPosition);

        foreach (Collider2D hit in hits)
        {
            if (hit.TryGetComponent(out E_ContainerSlot slot))
                return slot;
        }

        return null;
    }

    // Recomputes every container's row position (and the trailing empty slot right
    // below the last one) from the column's fixed anchor -- simpler and less error
    // prone than patching individual positions after an add/remove.
    private void RepositionColumn(int columnIndex)
    {
        Vector3 anchor = _columnAnchors[columnIndex];
        List<E_BallContainer> column = _columns[columnIndex];

        for (int row = 0; row < column.Count; row++)
            column[row].MoveTo(anchor + new Vector3(0f, -row * rowSpacing, 0f));

        _slots[columnIndex].transform.position = anchor + new Vector3(0f, -column.Count * rowSpacing, 0f);
    }
}
