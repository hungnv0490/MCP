using System.Collections.Generic;
using UnityEngine;

public class E_BoxesManager : MonoBehaviour
{
    public static E_BoxesManager Instance { get; private set; }

    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float boxSize = 1f;

    private readonly List<E_Box> spawnedBoxes = new List<E_Box>();

    private void Awake()
    {
        Instance = this;
    }

    public void DrawBoxes(Texture2D texture)
    {
        ClearBoxes();

        if (texture == null || spawnPoint == null || GameObjectPool.Instance == null)
            return;

        int width = texture.width;
        int height = texture.height;
        float centerX = (width - 1) * 0.5f;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color color = texture.GetPixel(x, y);
                if (color.a <= 0f)
                    continue;

                Vector3 position = spawnPoint.position + new Vector3((x - centerX) * boxSize, y * boxSize, 0f);
                E_Box box = GameObjectPool.Instance.Get<E_Box>(PoolType.Box, position, Quaternion.identity, transform);
                if (box == null)
                    continue;

                box.SetColor(color);
                spawnedBoxes.Add(box);
            }
        }
    }

    public void ClearBoxes()
    {
        foreach (var box in spawnedBoxes)
            GameObjectPool.Instance.ReturnToPool(box);
        spawnedBoxes.Clear();
    }
}
