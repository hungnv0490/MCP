using UnityEngine;

public class BallKillZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent(out Ball ball))
            return;

        GameObjectPool.Instance.ReturnToPool(ball);
    }
}
