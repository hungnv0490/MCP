using UnityEngine;

public class DeflectorPoint : MonoBehaviour
{
    [SerializeField] private float deflectForce = 3.5f;

    [Tooltip("Degrees excluded on each side of straight-up (90°). Keeps the deflected angle " +
             "away from dead-center so the ball never heads straight into the Obstacle's apex " +
             "and risks a symmetric bounce-back loop.")]
    [SerializeField] private float deadZoneHalfWidth = 10f;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(Ball.Tag) || !other.TryGetComponent(out Ball ball))
            return;

        if (ball.HasDeflected)
            return;

        ball.HasDeflected = true;

        float angle = (Random.value < 0.5f
            ? Random.Range(45f, 90f - deadZoneHalfWidth)
            : Random.Range(90f + deadZoneHalfWidth, 135f)) * Mathf.Deg2Rad;

        Vector3 direction = new(Mathf.Cos(angle), Mathf.Sin(angle), 0f);
        other.attachedRigidbody.AddForce(direction * deflectForce, ForceMode.VelocityChange);
    }
}
