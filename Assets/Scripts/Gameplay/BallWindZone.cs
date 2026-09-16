using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class BallWindZone : MonoBehaviour
{
    [SerializeField] private float force = 8f;

    [Tooltip("Gust cycle length in seconds. Zones sharing the same interval but opposite " +
             "startActive alternate, so only one ever pushes a ball at a time.")]
    [SerializeField] private float switchInterval = 1.5f;
    [SerializeField] private bool startActive = true;

    private bool IsActive =>
        switchInterval <= 0f
            ? startActive
            : Mathf.FloorToInt(Time.time / switchInterval) % 2 == 0 == startActive;

    private void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!IsActive)
            return;

        if (!other.CompareTag(Ball.Tag))
            return;

        // ForceMode2D has no mass-independent "Acceleration" option like 3D does,
        // so scale by mass to cancel out F=ma and get the same acceleration regardless of mass.
        Rigidbody2D otherRb = other.attachedRigidbody;
        otherRb.AddForce((Vector2)transform.right * force * otherRb.mass, ForceMode2D.Force);
    }

    private void OnDrawGizmos()
    {
        Vector3 direction = transform.right * Mathf.Sign(force == 0f ? 1f : force);
        Vector3 origin = transform.position;
        Vector3 tip = origin + direction * 1.5f;

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(origin, tip);
        Gizmos.DrawRay(tip, Quaternion.Euler(0f, 0f, 150f) * direction * 0.3f);
        Gizmos.DrawRay(tip, Quaternion.Euler(0f, 0f, -150f) * direction * 0.3f);
    }
}
