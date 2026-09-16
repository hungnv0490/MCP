using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Collider2D))]
public class BallContainer : MonoBehaviour
{
    [SerializeField] private SpriteRenderer background;
    [SerializeField] private TMP_Text countLabel;
    [SerializeField] private float flyDuration = 0.35f;
    [SerializeField] private float frontScale = 1.1f;

    public BallColorType ColorType { get; private set; }
    public int BallCount { get; private set; }
    public int ColumnIndex { get; private set; }

    // Raised the moment this container starts flying to the launch slot, so a
    // future ball-dispensing system can react without this class knowing about it.
    public event Action<BallContainer> Activated;

    // Raised once the container actually reaches the launch slot (right before
    // it despawns) -- this is what should trigger balls to start spawning.
    public event Action<BallContainer> Arrived;

    public void Setup(BallColorType colorType, int ballCount, int columnIndex)
    {
        ColorType = colorType;
        BallCount = ballCount;
        ColumnIndex = columnIndex;

        MaterialColorUtil.Apply(background, ColorPalette.Get(colorType));
        countLabel.text = ballCount.ToString();
    }

    public void SetFront(bool isFront)
    {
        transform.localScale = Vector3.one * (isFront ? frontScale : 1f);
    }

    public void MoveTo(Vector3 targetPosition, float duration)
    {
        StartCoroutine(LerpPosition(targetPosition, duration, null));
    }

    public void FlyToLaunchSlotAndDespawn(Vector3 launchSlotPosition)
    {
        Activated?.Invoke(this);
        StartCoroutine(LerpPosition(launchSlotPosition, flyDuration, () =>
        {
            Arrived?.Invoke(this);
            Destroy(gameObject);
        }));
    }

    private IEnumerator LerpPosition(Vector3 targetPosition, float duration, Action onComplete)
    {
        Vector3 start = transform.position;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            transform.position = Vector3.Lerp(start, targetPosition, t / duration);
            yield return null;
        }

        transform.position = targetPosition;
        onComplete?.Invoke();
    }

    private void OnMouseDown()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        BallContainerManager.Instance.TryActivate(this);
    }
}
