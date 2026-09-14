using System;
using UnityEngine;
using UnityEngine.UI;

public class E_PixcelItem : MonoBehaviour, IPoolable
{
    [SerializeField] private Image icon;
    [SerializeField] private Text fileName;
    [SerializeField] private Button button;

    private Action onClick;

    private void Awake()
    {
        button.onClick.AddListener(HandleClick);
    }

    public void Setup(Sprite sprite, string label, Action onClickCallback)
    {
        icon.sprite = sprite;
        icon.preserveAspect = true;
        fileName.text = label;
        onClick = onClickCallback;
    }

    private void HandleClick()
    {
        onClick?.Invoke();
    }

    public void OnSpawn()
    {
    }

    public void OnDespawn()
    {
        icon.sprite = null;
        fileName.text = string.Empty;
        onClick = null;
    }
}
