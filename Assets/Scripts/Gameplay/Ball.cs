using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PoolObject))]
public class Ball : MonoBehaviour, IPoolable
{
    // Single source of truth for the "Ball" tag, so other scripts can cheaply
    // reject non-ball colliders with CompareTag before paying for GetComponent.
    public const string Tag = "Ball";

    [SerializeField] private Renderer ballRenderer;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float gravityScale = 0.45f;

    [Header("Endgame Assist Steering")]
    [Tooltip("Only steers the ball when this many (or fewer) boxes are still alive in the scene.")]
    [SerializeField] private int assistBoxThreshold = 2;
    [Tooltip("Seconds a ball must survive without matching a box before assist starts.")]
    [SerializeField] private float assistDelay = 3f;
    [Tooltip("Seconds after assistDelay for the steering force to ramp from 0 to full strength.")]
    [SerializeField] private float assistRampTime = 4f;
    [SerializeField] private float assistForce = 1.5f;

    private Collider ballCollider;
    private PoolObject poolObject;
    private bool passedBottomWall;
    private float aliveTime;
    private float velocityYBeforeStep;

    public BallColorType ColorType { get; private set; }
    public bool HasDeflected { get; set; }

    // Cached instead of TryGetComponent<PoolObject>() on every collision check
    // (Box.OnCollisionEnter reads this once per hit to guard against double-consumption).
    public bool IsPooled => poolObject.IsPooled;

    private void Awake()
    {
        rb.useGravity = false;
        rb.sleepThreshold = 0f; // Balls bounce forever; never let PhysX put them to sleep.
        ballCollider = GetComponent<Collider>();
        poolObject = GetComponent<PoolObject>();
    }

    private void FixedUpdate()
    {
        // Snapshot of vertical velocity before this step's forces/collisions resolve
        // (AddForce is deferred until the physics solve, so this still reads last
        // step's post-collision value). Used to restore momentum after tube-guide hits.
        velocityYBeforeStep = rb.linearVelocity.y;

        rb.AddForce(Physics.gravity * gravityScale, ForceMode.Acceleration);

        // One-way gate: BottomWallCenter is ignored while the ball is passing through
        // it from below; once the ball has fully cleared it, treat it as a solid Bounds wall.
        if (!passedBottomWall && BottomWallCenter.Instance != null &&
            ballCollider.bounds.min.y >= BottomWallCenter.Instance.TopY)
        {
            passedBottomWall = true;
            Physics.IgnoreCollision(ballCollider, BottomWallCenter.Instance.WallCollider, false);
        }

        aliveTime += Time.fixedDeltaTime;
        ApplyAssistSteering();
    }

    // Subtle endgame nudge: when only a couple of boxes remain, dumb luck bouncing can take
    // forever to line up a hit. Once a ball has wandered past assistDelay with no match, ease
    // in a gentle pull toward its nearest same-color box so the round always resolves, without
    // the force being strong or sudden enough to read as an obvious homing missile.
    private void ApplyAssistSteering()
    {
        if (Box.Active.Count > assistBoxThreshold || aliveTime < assistDelay)
            return;

        Box target = FindNearestMatchingBox();
        if (target == null)
            return;

        Vector3 toTarget = target.transform.position - transform.position;
        toTarget.z = 0f;

        if (toTarget.sqrMagnitude < 0.01f)
            return;

        float ramp = Mathf.Clamp01((aliveTime - assistDelay) / assistRampTime);
        rb.AddForce(toTarget.normalized * (assistForce * ramp), ForceMode.Acceleration);
    }

    private Box FindNearestMatchingBox()
    {
        Box nearest = null;
        float nearestSqrDistance = float.MaxValue;

        foreach (Box box in Box.Active)
        {
            if (box.ColorType != ColorType)
                continue;

            float sqrDistance = (box.transform.position - transform.position).sqrMagnitude;
            if (sqrDistance < nearestSqrDistance)
            {
                nearestSqrDistance = sqrDistance;
                nearest = box;
            }
        }

        return nearest;
    }

    public void Setup(BallColorType colorType, Vector3 velocity)
    {
        ColorType = colorType;
        MaterialColorUtil.Apply(ballRenderer, ColorPalette.Get(colorType));

        rb.linearVelocity = velocity;
        rb.angularVelocity = Vector3.zero;
    }

    public void OnMatchedBox()
    {
        GameObjectPool.Instance.ReturnToPool(this);
    }

    public void OnSpawn()
    {
        HasDeflected = false;
        passedBottomWall = false;
        aliveTime = 0f;

        if (BottomWallCenter.Instance != null)
            Physics.IgnoreCollision(ballCollider, BottomWallCenter.Instance.WallCollider, true);
    }

    public void OnDespawn()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }
}
