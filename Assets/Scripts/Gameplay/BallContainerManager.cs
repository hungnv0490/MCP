using System.Collections.Generic;
using UnityEngine;

// Owns the per-column queues of BallContainer and reacts to touches: only the
// front (index 0) of a column is interactable. Activating it removes it from
// the queue and shifts every remaining container in that column forward by
// one slot. Populated by BallContainerSpawner via RegisterColumn.
public class BallContainerManager : MonoBehaviour
{
    public static BallContainerManager Instance { get; private set; }

    [SerializeField] private Transform launchSlot;
    [SerializeField] private float rowSpacing = 1.2f;
    [SerializeField] private float shiftDuration = 0.2f;

    private readonly Dictionary<int, List<BallContainer>> _columns = new();

    private void Awake()
    {
        Instance = this;
    }

    public void RegisterColumn(int columnIndex, List<BallContainer> containers)
    {
        _columns[columnIndex] = containers;

        foreach (BallContainer container in containers)
            container.Arrived += OnContainerArrived;

        if (containers.Count > 0)
            containers[0].SetFront(true);
    }

    private void OnContainerArrived(BallContainer container)
    {
        BallSpawner.Instance.SpawnBalls(container.ColorType, container.BallCount);
    }

    public void TryActivate(BallContainer container)
    {
        if (!_columns.TryGetValue(container.ColumnIndex, out List<BallContainer> column))
            return;

        if (column.Count == 0 || column[0] != container)
            return; // Only the front of the column is interactable.

        column.RemoveAt(0);
        container.FlyToLaunchSlotAndDespawn(launchSlot.position);

        ShiftColumnForward(column);
    }

    private void ShiftColumnForward(List<BallContainer> column)
    {
        for (int row = 0; row < column.Count; row++)
        {
            Vector3 targetPosition = column[row].transform.position + Vector3.up * rowSpacing;
            column[row].MoveTo(targetPosition, shiftDuration);
        }

        if (column.Count > 0)
            column[0].SetFront(true);
    }
}
