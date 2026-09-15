using UnityEngine;

public class BoxGridSpawner : MonoBehaviour
{
    [SerializeField] private Transform origin;
    [SerializeField] private int columns = 5;
    [SerializeField] private int rows = 2;
    [SerializeField] private float spacing = 1.1f;

    private void Start()
    {
        SpawnGrid();
    }

    private void SpawnGrid()
    {
        if (GameObjectPool.Instance == null)
            return;

        float startX = -(columns - 1) * spacing * 0.5f;

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                Vector3 position = origin.position + new Vector3(startX + col * spacing, row * spacing, 0f);
                Box box = GameObjectPool.Instance.Get<Box>(PoolType.GameBox, position, Quaternion.identity);

                if (box == null)
                    continue;

                box.Setup(ColorPalette.GetRandom());
            }
        }
    }
}
