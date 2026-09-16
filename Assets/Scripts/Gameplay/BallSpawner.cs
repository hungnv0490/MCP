using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallSpawner : MonoBehaviour
{
    public static BallSpawner Instance { get; private set; }

    [SerializeField] private Transform spawnPoint;
    [SerializeField] private BoxGridSpawner boxGridSpawner;
    [SerializeField] private float launchSpeed = 10f;
    [SerializeField] private float spawnInterval = 0.8f;

    private void Awake()
    {
        Instance = this;

        if (boxGridSpawner == null)
            boxGridSpawner = GetComponent<BoxGridSpawner>();
    }

    private void Start()
    {
        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        // Wait a frame so BoxGridSpawner.Start() has spawned its boxes first.
        yield return null;

        List<BallColorType> colors = new(boxGridSpawner.SpawnedColors);
        Shuffle(colors);

        foreach (BallColorType color in colors)
        {
            SpawnBall(color);
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnBall(BallColorType colorType)
    {
        if (GameObjectPool.Instance == null)
            return;

        Ball ball = GameObjectPool.Instance.Get<Ball>(PoolType.Ball, spawnPoint.position, Quaternion.identity);

        if (ball == null)
            return;

        ball.Setup(colorType, Vector2.up * launchSpeed);
    }

    // Dispenses `count` balls of `colorType` from the spawn point, one per spawnInterval.
    // Called by BallContainerManager when a queued container arrives at the launch slot.
    public void SpawnBalls(BallColorType colorType, int count)
    {
        StartCoroutine(SpawnBallsRoutine(colorType, count));
    }

    private IEnumerator SpawnBallsRoutine(BallColorType colorType, int count)
    {
        for (int i = 0; i < count; i++)
        {
            SpawnBall(colorType);
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private static void Shuffle(IList<BallColorType> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
