using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Ball : MonoBehaviour, IPoolable
{
    [SerializeField] private Renderer ballRenderer;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float gravityScale = 0.45f;

    public BallColorType ColorType { get; private set; }
    public bool HasDeflected { get; set; }

    private void Awake()
    {
        rb.useGravity = false;
    }

    private void FixedUpdate()
    {
        rb.AddForce(Physics.gravity * gravityScale, ForceMode.Acceleration);
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
    }

    public void OnDespawn()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }
}
