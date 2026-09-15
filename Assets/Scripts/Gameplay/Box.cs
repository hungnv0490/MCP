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
