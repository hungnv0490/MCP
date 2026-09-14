using UnityEngine;
using UnityEngine.EventSystems;

public class E_Box : MonoBehaviour, IPoolable
{
    [SerializeField] private SpriteRenderer spriteRenderer;

    public void SetColor(Color color)
    {
        spriteRenderer.color = color;
    }

    private void OnMouseDown()
    {
        HandleClick();
    }

    public void HandleClick()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        if (E_BoxColorPopup.Instance != null)
            E_BoxColorPopup.Instance.TryPaint(this);
    }

    public void OnSpawn()
    {
    }

    public void OnDespawn()
    {
    }
}
