using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

// Lets a designer drag the camera up/down (left mouse button, held over empty
// space -- not while grabbing a container or box) to see content taller than the
// screen, e.g. a long ball-container column or a tall box grid. The vertical
// range is clamped to the actual generated content, so once everything already
// fits on screen the drag is a no-op. The mouse scroll wheel zooms the camera in
// and out (adjusting orthographic size), clamped to a configurable size range and
// re-clamped against the pan bounds afterward.
public class E_CameraPanController : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    [SerializeField] private float edgeMargin = 1f;

    [Tooltip("Orthographic size removed per unit of mouse-wheel scroll.")]
    [SerializeField] private float zoomSpeed = 0.02f;
    [SerializeField] private float minOrthographicSize = 2f;
    [SerializeField] private float maxOrthographicSize = 30f;

    private bool isPanning;
    private Vector3 lastWorldPoint;

    private void Awake()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;
    }

    private void Update()
    {
        if (targetCamera == null || Mouse.current == null)
            return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
            TryBeginPan();
        else if (Mouse.current.leftButton.wasReleasedThisFrame)
            isPanning = false;

        if (isPanning)
            ContinuePan();

        TryZoom();
    }

    private void TryZoom()
    {
        float scroll = Mouse.current.scroll.ReadValue().y;
        if (Mathf.Approximately(scroll, 0f))
            return;

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        // Scrolling up (positive) zooms in (smaller orthographic size); down zooms out.
        float size = targetCamera.orthographicSize - scroll * zoomSpeed;
        targetCamera.orthographicSize = Mathf.Clamp(size, minOrthographicSize, maxOrthographicSize);

        // The valid pan range depends on orthographic size, so re-clamp the current
        // position -- otherwise zooming out could reveal empty space past the content.
        Vector3 position = targetCamera.transform.position;
        position.y = ClampY(position.y);
        targetCamera.transform.position = position;
    }

    private void TryBeginPan()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        Vector3 worldPoint = targetCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());

        // Let E_BallContainer/E_Box handle their own OnMouseDown drag instead of
        // also starting a camera pan underneath them.
        Collider2D hit = Physics2D.OverlapPoint(worldPoint);
        if (hit != null && (hit.GetComponent<E_BallContainer>() != null || hit.GetComponent<E_Box>() != null))
            return;

        isPanning = true;
        lastWorldPoint = worldPoint;
    }

    private void ContinuePan()
    {
        Vector3 worldPoint = targetCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        float deltaY = worldPoint.y - lastWorldPoint.y;

        Vector3 position = targetCamera.transform.position;
        position.y = ClampY(position.y - deltaY);
        targetCamera.transform.position = position;

        // Recompute against the camera's new (possibly clamped) position so the
        // drag doesn't feel like it "catches up" after hitting a bound.
        lastWorldPoint = targetCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
    }

    private float ClampY(float y)
    {
        if (!TryGetContentBoundsY(out float minContentY, out float maxContentY))
            return y;

        float halfHeight = targetCamera.orthographicSize;
        float minCameraY = minContentY + halfHeight - edgeMargin;
        float maxCameraY = maxContentY - halfHeight + edgeMargin;

        if (minCameraY > maxCameraY)
            return (minContentY + maxContentY) * 0.5f;

        return Mathf.Clamp(y, minCameraY, maxCameraY);
    }

    private static bool TryGetContentBoundsY(out float minY, out float maxY)
    {
        minY = float.MaxValue;
        maxY = float.MinValue;
        bool any = false;

        if (E_BoxesManager.Instance != null && E_BoxesManager.Instance.TryGetBoxesBoundsY(out float boxMinY, out float boxMaxY))
        {
            minY = Mathf.Min(minY, boxMinY);
            maxY = Mathf.Max(maxY, boxMaxY);
            any = true;
        }

        if (E_BallContainerManager.Instance != null && E_BallContainerManager.Instance.TryGetColumnsBoundsY(out float colMinY, out float colMaxY))
        {
            minY = Mathf.Min(minY, colMinY);
            maxY = Mathf.Max(maxY, colMaxY);
            any = true;
        }

        return any;
    }
}
