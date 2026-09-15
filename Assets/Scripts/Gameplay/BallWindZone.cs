using UnityEngine;

[RequireComponent(typeof(Collider))]
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
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerStay(Collider other)
    {
        if (!IsActive)
            return;

        if (!other.TryGetComponent<Ball>(out _))
            return;

        other.attachedRigidbody.AddForce(transform.right * force, ForceMode.Acceleration);
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
