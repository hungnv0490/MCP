using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Ball : MonoBehaviour, IPoolable
{
    [SerializeField] private Renderer ballRenderer;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float gravityScale = 0.45f;

    private Collider ballCollider;
    private bool passedBottomWall;

    public BallColorType ColorType { get; private set; }
    public bool HasDeflected { get; set; }

    private void Awake()
    {
        rb.useGravity = false;
        rb.sleepThreshold = 0f; // Balls bounce forever; never let PhysX put them to sleep.
        ballCollider = GetComponent<Collider>();

        // Balls never collide with each other, only with Boxes and Bounds walls.
        Physics.IgnoreLayerCollision(gameObject.layer, gameObject.layer, true);
    }

    private void FixedUpdate()
    {
        rb.AddForce(Physics.gravity * gravityScale, ForceMode.Acceleration);

        // One-way gate: BottomWallCenter is ignored while the ball is passing through
        // it from below; once the ball has fully cleared it, treat it as a solid Bounds wall.
        if (!passedBottomWall && BottomWallCenter.Instance != null &&
            ballCollider.bounds.min.y >= BottomWallCenter.Instance.TopY)
        {
            passedBottomWall = true;
            Physics.IgnoreCollision(ballCollider, BottomWallCenter.Instance.WallCollider, false);
        }
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

        if (BottomWallCenter.Instance != null)
            Physics.IgnoreCollision(ballCollider, BottomWallCenter.Instance.WallCollider, true);
    }

    public void OnDespawn()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }
}
