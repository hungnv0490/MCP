using System;
using System.Collections.Generic;
using UnityEngine;

public class E_BoxesManager : MonoBehaviour
{
    public static E_BoxesManager Instance { get; private set; }

    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float boxSize = 1f;

    private readonly List<E_Box> spawnedBoxes = new List<E_Box>();

    // Raised after DrawBoxes finishes spawning every box, so listeners (e.g. the
    // ball-container preview spawner) can react without E_BoxesManager needing to
    // know anything about them.
    public event Action BoxesDrawn;

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
                E_Box box = GameObjectPool.Instance.Get<E_Box>(PoolType.E_Box, position, Quaternion.identity, transform);
                if (box == null)
                    continue;

                box.SetColor(color);
                spawnedBoxes.Add(box);
            }
        }

        BoxesDrawn?.Invoke();
    }

    public void ClearBoxes()
    {
        foreach (var box in spawnedBoxes)
            GameObjectPool.Instance.ReturnToPool(box);
        spawnedBoxes.Clear();
    }

    // Tallies currently-drawn boxes by their exact color, so a caller can figure out
    // how many boxes of each color exist without reaching into the private box list.
    public Dictionary<Color, int> GetColorCounts()
    {
        Dictionary<Color, int> counts = new();

        foreach (E_Box box in spawnedBoxes)
        {
            Color color = box.CurrentColor;
            counts[color] = counts.TryGetValue(color, out int count) ? count + 1 : 1;
        }

        return counts;
    }

    // Vertical extent of the currently-drawn boxes, used by E_CameraPanController
    // to know how far it's allowed to pan so the whole box grid stays reachable.
    public bool TryGetBoxesBoundsY(out float minY, out float maxY)
    {
        minY = float.MaxValue;
        maxY = float.MinValue;

        foreach (E_Box box in spawnedBoxes)
        {
            float y = box.transform.position.y;
            minY = Mathf.Min(minY, y);
            maxY = Mathf.Max(maxY, y);
        }

        return spawnedBoxes.Count > 0;
    }
}
