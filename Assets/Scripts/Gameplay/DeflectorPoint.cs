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

        float angle = Random.Range(45f, 135f) * Mathf.Deg2Rad;
        Vector3 direction = new(Mathf.Cos(angle), Mathf.Sin(angle), 0f);
        other.attachedRigidbody.AddForce(direction * deflectForce, ForceMode.VelocityChange);
    }
}
