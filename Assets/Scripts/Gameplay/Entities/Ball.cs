using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(PoolObject))]
public class Ball : MonoBehaviour, IPoolable
{
    // Single source of truth for the "Ball" tag, so other scripts can cheaply
    // reject non-ball colliders with CompareTag before paying for GetComponent.
    public const string Tag = "Ball";

    [SerializeField] private string instanceId;
    [SerializeField] private Renderer ballRenderer;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private float gravityScale = 0.45f;

    [Header("Tube Physics Material Swap")]
    [Tooltip("Zero bounciness/friction, assigned while the ball travels through the tube " +
             "(DeflectorPoint/BallWindZone rely on it not bouncing off tube walls).")]
    [SerializeField] private PhysicsMaterial2D tubeMaterial;
    [Tooltip("The ball's normal bouncy material, restored once it exits the tube into the box area.")]
    [SerializeField] private PhysicsMaterial2D normalMaterial;

    // "BallInTube" collides with itself (unlike the normal "Ball" layer, which ignores
    // ball-ball collisions) so balls queued in the narrow, frictionless tube bump into
    // each other instead of visibly overlapping. Swapped back to the ball's original
    // layer in ExitTube() once it reaches the open box area.
    private static int tubeLayer;
    private int defaultLayer;

    [Header("Endgame Assist Steering")]
    [Tooltip("Only steers the ball when this many (or fewer) boxes are still alive in the scene.")]
    [SerializeField] private int assistBoxThreshold = 2;
    [Tooltip("Seconds a ball must survive without matching a box before assist starts.")]
    [SerializeField] private float assistDelay = 3f;
    [Tooltip("Seconds after assistDelay for the steering force to ramp from 0 to full strength.")]
    [SerializeField] private float assistRampTime = 4f;
    [SerializeField] private float assistForce = 1.5f;

    private Collider2D ballCollider;
    private PoolObject poolObject;
    private bool passedBottomWall;
    private float aliveTime;

    // Tracks which DeflectorPoint/TubeExitPoint instances this ball has already
    // triggered, so a journey through several of them (entrance, fork, exit) never
    // re-fires the same one twice, without needing a separate named bool per junction.
    private readonly HashSet<Object> consumedJunctions = new();

    public BallColorType ColorType { get; private set; }

    // Cached instead of TryGetComponent<PoolObject>() on every collision check
    // (Box.OnCollisionEnter2D reads this once per hit to guard against double-consumption).
    public bool IsPooled => poolObject.IsPooled;

    // Returns true (and remembers it) the first time this specific junction is passed
    // by this ball; false on every subsequent call for the same junction/ball pairing.
    public bool TryConsumeJunction(Object junction) => consumedJunctions.Add(junction);

    private void Awake()
    {
        instanceId = GetEntityId().ToString();
        rb.gravityScale = gravityScale; // Physics2D applies Physics2D.gravity * gravityScale automatically each step.
        rb.sleepMode = RigidbodySleepMode2D.NeverSleep; // Balls bounce forever; never let PhysX put them to sleep.
        ballCollider = GetComponent<Collider2D>();
        poolObject = GetComponent<PoolObject>();
        defaultLayer = gameObject.layer;
        tubeLayer = LayerMask.NameToLayer("BallInTube");
    }

    private void FixedUpdate()
    {
        // One-way gate: BottomWallCenter is ignored while the ball is passing through
        // it from below; once the ball has fully cleared it, treat it as a solid Bounds wall.
        if (!passedBottomWall && BottomWallCenter.Instance != null &&
            ballCollider.bounds.min.y >= BottomWallCenter.Instance.TopY)
        {
            passedBottomWall = true;
            Physics2D.IgnoreCollision(ballCollider, BottomWallCenter.Instance.WallCollider, false);
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

        Vector2 toTarget = target.transform.position - transform.position;

        if (toTarget.sqrMagnitude < 0.01f)
            return;

        float ramp = Mathf.Clamp01((aliveTime - assistDelay) / assistRampTime);

        // ForceMode2D has no mass-independent "Acceleration" option like 3D does,
        // so scale by mass to cancel out F=ma and get the same acceleration regardless of mass.
        rb.AddForce(toTarget.normalized * (assistForce * ramp) * rb.mass, ForceMode2D.Force);
    }

    private Box FindNearestMatchingBox()
    {
        Box nearest = null;
        float nearestSqrDistance = float.MaxValue;

        foreach (Box box in Box.Active)
        {
            if (box.ColorType != ColorType)
                continue;

            float sqrDistance = ((Vector2)box.transform.position - (Vector2)transform.position).sqrMagnitude;
            if (sqrDistance < nearestSqrDistance)
            {
                nearestSqrDistance = sqrDistance;
                nearest = box;
            }
        }

        return nearest;
    }

    public void Setup(BallColorType colorType, Vector2 velocity)
    {
        ColorType = colorType;
        MaterialColorUtil.Apply(ballRenderer, ColorPalette.Get(colorType));

        rb.linearVelocity = velocity;
        rb.angularVelocity = 0f;
    }

    public void OnMatchedBox()
    {
        GameObjectPool.Instance.ReturnToPool(this);
    }

    // Called by TubeExitPoint once the ball crosses into the main box area,
    // restoring the elastic bounce used for the actual box-breaking gameplay.
    public void ExitTube()
    {
        ballCollider.sharedMaterial = normalMaterial;
        gameObject.layer = defaultLayer;
    }

    public void OnSpawn()
    {
        consumedJunctions.Clear();
        passedBottomWall = false;
        aliveTime = 0f;

        // Every launch starts by traveling up through the tube, which needs the
        // ball to not bounce off its walls (see TubeExitPoint for the handoff back).
        ballCollider.sharedMaterial = tubeMaterial;
        gameObject.layer = tubeLayer;

        if (BottomWallCenter.Instance != null)
            Physics2D.IgnoreCollision(ballCollider, BottomWallCenter.Instance.WallCollider, true);
    }

    public void OnDespawn()
    {
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
    }
}
