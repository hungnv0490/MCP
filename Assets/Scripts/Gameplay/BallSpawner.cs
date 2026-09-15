using System.Collections;
using UnityEngine;

public class BallSpawner : MonoBehaviour
{
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float launchSpeed = 10f;
    [SerializeField] private float spawnInterval = 0.8f;
    [SerializeField] private int ballCount = 10;

    private void Start()
    {
        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        for (int i = 0; i < ballCount; i++)
        {
            SpawnBall();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnBall()
    {
        if (GameObjectPool.Instance == null)
            return;

        Ball ball = GameObjectPool.Instance.Get<Ball>(PoolType.Ball, spawnPoint.position, Quaternion.identity);

        if (ball == null)
            return;

        ball.Setup(ColorPalette.GetRandom(), Vector3.up * launchSpeed);
    }
}
