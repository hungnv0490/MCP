using System.Collections.Generic;
using UnityEngine;

// Builds the initial grid of BallContainer columns and hands each column
// over to BallContainerManager, which owns queue/touch behavior from then on.
public class BallContainerSpawner : MonoBehaviour
{
    [SerializeField] private BallContainer containerPrefab;
    [SerializeField] private Transform origin;
    [SerializeField] private int columns = 5;
    [SerializeField] private int rows = 3;
    [SerializeField] private float columnSpacing = 1.2f;
    [SerializeField] private float rowSpacing = 1.2f;
    [SerializeField] private Vector2Int ballCountRange = new(10, 90);

    private void Start()
    {
        SpawnGrid();
    }

    private void SpawnGrid()
    {
        float startX = -(columns - 1) * columnSpacing * 0.5f;

        for (int col = 0; col < columns; col++)
        {
            List<BallContainer> column = new();

            for (int row = 0; row < rows; row++)
            {
                Vector3 position = origin.position + new Vector3(startX + col * columnSpacing, -row * rowSpacing, 0f);
                BallContainer container = Instantiate(containerPrefab, position, Quaternion.identity, transform);

                BallColorType color = ColorPalette.GetRandom();
                int count = Random.Range(ballCountRange.x, ballCountRange.y + 1);
                container.Setup(color, count, col);

                column.Add(container);
            }

            BallContainerManager.Instance.RegisterColumn(col, column);
        }
    }
}
