using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class BottomWallCenter : MonoBehaviour
{
    public static BottomWallCenter Instance { get; private set; }

    private Collider2D wallCollider;

    public Collider2D WallCollider => wallCollider;
    public float TopY => wallCollider.bounds.max.y;

    private void Awake()
    {
        Instance = this;
        wallCollider = GetComponent<Collider2D>();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
