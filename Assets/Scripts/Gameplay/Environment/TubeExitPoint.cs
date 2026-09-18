using System.Collections;
using UnityEngine;

// Sits at the top of a side tube (E or F), where it feeds into the main box area.
// Fires the ball inward/upward with an angled kick, hands it back its normal bouncy
// material (it was quiet/non-bouncy for the tube), and briefly ignores collision with
// the boundary walls right at this seam so the ball can cross into the play area
// before those walls become solid again.
public class TubeExitPoint : MonoBehaviour
{
    [Tooltip("Random angle range (degrees, 0=+X/right, 90=+Y/up, 180=-X/left) the exit kick is picked from.")]
    [SerializeField] private float minAngleDeg = 20f;
    [SerializeField] private float maxAngleDeg = 80f;
    [SerializeField] private float launchForce = 6f;

    [Tooltip("How long collision with boundaryWallsToIgnore stays off after exiting, giving the " +
             "ball time to fully cross the seam before those walls become solid again.")]
    [SerializeField] private float ignoreCollisionDuration = 0.3f;

    [SerializeField] private Collider[] boundaryWallsToIgnore;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(Ball.Tag) || !other.TryGetComponent(out Ball ball))
            return;

        if (!ball.TryConsumeJunction(this))
            return;

        ball.ExitTube();

        Rigidbody rb = other.attachedRigidbody;

        foreach (Collider wall in boundaryWallsToIgnore)
            Physics.IgnoreCollision(other, wall, true);

        float angle = Random.Range(minAngleDeg, maxAngleDeg) * Mathf.Deg2Rad;
        Vector3 direction = new(Mathf.Cos(angle), Mathf.Sin(angle), 0f);

        rb.linearVelocity = Vector3.zero;
        rb.AddForce(direction * launchForce * rb.mass, ForceMode.Impulse);

        StartCoroutine(RestoreCollisionAfterDelay(other));
    }

    private IEnumerator RestoreCollisionAfterDelay(Collider ballCollider)
    {
        yield return new WaitForSeconds(ignoreCollisionDuration);

        if (ballCollider == null)
            yield break;

        foreach (Collider wall in boundaryWallsToIgnore)
            Physics.IgnoreCollision(ballCollider, wall, false);
    }
}
