using UnityEngine;

[RequireComponent(typeof(Collider))]
public class BottomWallCenter : MonoBehaviour
{
    public static BottomWallCenter Instance { get; private set; }

    private Collider wallCollider;

    public Collider WallCollider => wallCollider;
    public float TopY => wallCollider.bounds.max.y;

    private void Awake()
    {
        Instance = this;
        wallCollider = GetComponent<Collider>();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
