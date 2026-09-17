using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

// Editor-preview counterpart of the runtime BallContainer: represents "this many
// balls of this color" so a game designer can see, while authoring a level from a
// pixel image, whether the ball supply matches the drawn boxes. No launch-slot/
// ball-spawning behaviour here -- the only interaction is dragging it into an
// empty E_ContainerSlot in another column to hand-tune difficulty.
[RequireComponent(typeof(Collider2D))]
public class E_BallContainer : MonoBehaviour, IPoolable
{
    [SerializeField] private SpriteRenderer background;
    [SerializeField] private TMP_Text countLabel;

    public Color ColorValue { get; private set; }
    public int BallCount { get; private set; }
    public int ColumnIndex { get; private set; }

    private bool isDragging;
    private Vector3 originalPosition;

    private void Awake()
    {
        // See BallContainer.Awake -- TextMeshPro's MeshRenderer doesn't expose
        // Sorting Layer/Order in Layer in the Normal inspector, so pin it above
        // the background sprite here instead.
        Renderer countLabelRenderer = countLabel.GetComponent<Renderer>();
        countLabelRenderer.sortingLayerID = background.sortingLayerID;
        countLabelRenderer.sortingOrder = background.sortingOrder + 1;
    }

    public void Setup(Color color, int ballCount, int columnIndex)
    {
        ColorValue = color;
        BallCount = ballCount;
        ColumnIndex = columnIndex;

        MaterialColorUtil.Apply(background, color);
        countLabel.text = ballCount.ToString();
    }

    public void SetColumnIndex(int columnIndex)
    {
        ColumnIndex = columnIndex;
    }

    // Adjusts the ball count in place (e.g. a single box getting repainted away
    // from/into this color) without touching color/column -- distinct from Setup,
    // which is a full (re)initialization when the container is (re)spawned.
    public void SetBallCount(int ballCount)
    {
        BallCount = ballCount;
        countLabel.text = ballCount.ToString();
    }

    public void MoveTo(Vector3 targetPosition)
    {
        transform.position = targetPosition;
    }

    private void Update()
    {
        if (!isDragging || Mouse.current == null)
            return;

        Vector3 worldPoint = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        transform.position = new Vector3(worldPoint.x, worldPoint.y, originalPosition.z);
    }

    private void OnMouseDown()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        isDragging = true;
        originalPosition = transform.position;
    }

    private void OnMouseUp()
    {
        if (!isDragging)
            return;

        isDragging = false;
        E_BallContainerManager.Instance.TryDrop(this, transform.position, originalPosition);
    }

    public void OnSpawn()
    {
        isDragging = false;
    }

    public void OnDespawn()
    {
        isDragging = false;
    }
}
