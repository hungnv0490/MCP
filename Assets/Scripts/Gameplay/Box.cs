using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PoolObject))]
public class Box : MonoBehaviour, IPoolable
{
    [SerializeField] private Renderer boxRenderer;

    private static readonly List<Box> _active = new();

    // Currently spawned boxes still waiting to be matched. Used by Ball's endgame
    // assist steering to find the nearest surviving target without a scene-wide search.
    public static IReadOnlyList<Box> Active => _active;

    private PoolObject poolObject;

    public BallColorType ColorType { get; private set; }

    private void Awake()
    {
        poolObject = GetComponent<PoolObject>();
    }

    public void Setup(BallColorType colorType)
    {
        ColorType = colorType;
        MaterialColorUtil.Apply(boxRenderer, ColorPalette.Get(colorType));
    }

    [RuntimeInitializeOnLoadMethod]
    private static void ResetStatics()
    {
        _active.Clear();
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Two balls landing on the same box within the same physics step both raise
        // OnCollisionEnter before either's deactivation takes effect. Once this box has
        // already been consumed, bail out so the second ball isn't wrongly destroyed too.
        if (poolObject.IsPooled)
            return;

        if (!collision.collider.CompareTag(Ball.Tag) || !collision.collider.TryGetComponent(out Ball ball))
            return;

        if (ball.ColorType != ColorType)
            return;

        // A ball touching two adjacent boxes in the same physics step must only
        // consume the first one it matches, not destroy both from a single ball.
        if (ball.IsPooled)
            return;

        ball.OnMatchedBox();
        GameObjectPool.Instance.ReturnToPool(this);
    }

    public void OnSpawn()
    {
        _active.Add(this);
    }

    public void OnDespawn()
    {
        _active.Remove(this);
    }
}
