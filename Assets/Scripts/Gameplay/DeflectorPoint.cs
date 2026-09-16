using UnityEngine;

// A one-shot directional kick: the first time a ball passes through, it gets an
// instant impulse angled somewhere in [minAngleDeg, maxAngleDeg] (optionally split
// around a dead zone at the midpoint, so it never fires straight down the middle).
// Reusable at any tube junction -- entrance, forks, or an exit into the box area --
// since each instance tracks "have I already fired for this ball" independently via
// Ball.TryConsumeJunction, so passing several of these in sequence never conflicts.
public class DeflectorPoint : MonoBehaviour
{
    [SerializeField] private float deflectForce = 3.5f;

    [Tooltip("Random angle range (degrees, 0=+X/right, 90=+Y/up, 180=-X/left) the kick is picked from.")]
    [SerializeField] private float minAngleDeg = 45f;
    [SerializeField] private float maxAngleDeg = 135f;

    [Tooltip("Degrees excluded on each side of the midpoint between min/max, splitting the pick " +
             "into two halves (e.g. to avoid firing straight up the middle). 0 disables splitting.")]
    [SerializeField] private float deadZoneHalfWidth = 10f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(Ball.Tag) || !other.TryGetComponent(out Ball ball))
            return;

        if (!ball.TryConsumeJunction(this))
            return;

        float angle = PickAngleDeg() * Mathf.Deg2Rad;
        Vector2 direction = new(Mathf.Cos(angle), Mathf.Sin(angle));

        // ForceMode2D has no direct "VelocityChange" (mass-independent instant kick) like 3D --
        // Impulse divides by mass to get a velocity change, so multiply by mass first to cancel
        // that out and land on the same mass-independent instant velocity change.
        other.attachedRigidbody.AddForce(direction * deflectForce * other.attachedRigidbody.mass, ForceMode2D.Impulse);
    }

    private float PickAngleDeg()
    {
        if (deadZoneHalfWidth <= 0f)
            return Random.Range(minAngleDeg, maxAngleDeg);

        float mid = (minAngleDeg + maxAngleDeg) * 0.5f;

        return Random.value < 0.5f
            ? Random.Range(minAngleDeg, mid - deadZoneHalfWidth)
            : Random.Range(mid + deadZoneHalfWidth, maxAngleDeg);
    }
}
