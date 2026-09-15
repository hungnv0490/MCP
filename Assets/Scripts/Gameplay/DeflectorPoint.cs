using UnityEngine;

public class DeflectorPoint : MonoBehaviour
{
    [SerializeField] private float deflectForce = 3.5f;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent(out Ball ball))
            return;

        if (ball.HasDeflected)
            return;

        ball.HasDeflected = true;

        float sign = Random.value < 0.5f ? -1f : 1f;
        other.attachedRigidbody.AddForce(Vector3.right * sign * deflectForce, ForceMode.VelocityChange);
    }
}
