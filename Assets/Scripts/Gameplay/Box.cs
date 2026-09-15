using UnityEngine;

public class Box : MonoBehaviour, IPoolable
{
    [SerializeField] private Renderer boxRenderer;

    public BallColorType ColorType { get; private set; }

    public void Setup(BallColorType colorType)
    {
        ColorType = colorType;
        MaterialColorUtil.Apply(boxRenderer, ColorPalette.Get(colorType));
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.collider.TryGetComponent(out Ball ball))
            return;

        if (ball.ColorType != ColorType)
            return;

        // A ball touching two adjacent boxes in the same physics step must only
        // consume the first one it matches, not destroy both from a single ball.
        if (ball.TryGetComponent(out PoolObject ballPoolObject) && ballPoolObject.IsPooled)
            return;

        ball.OnMatchedBox();
        GameObjectPool.Instance.ReturnToPool(this);
    }

    public void OnSpawn()
    {
    }

    public void OnDespawn()
    {
    }
}
