using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class LoadPixcelsPopup : MonoBehaviour
{
    [SerializeField] private RectTransform content;
    [SerializeField] private Button closeButton;
    [SerializeField] private string pixcelsFolder = "Assets/Textures/Pixcels";

    private readonly List<E_PixcelItem> spawnedItems = new List<E_PixcelItem>();

    private void Awake()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(Hide);
    }

    public void Show()
    {
        gameObject.SetActive(true);
        Populate();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void Populate()
    {
        foreach (var item in spawnedItems)
            GameObjectPool.Instance.ReturnToPool(item);
        spawnedItems.Clear();

#if UNITY_EDITOR
        if (!Directory.Exists(pixcelsFolder))
            return;

        var files = new List<string>();
        files.AddRange(Directory.GetFiles(pixcelsFolder, "*.png"));
        files.AddRange(Directory.GetFiles(pixcelsFolder, "*.jpg"));
        files.AddRange(Directory.GetFiles(pixcelsFolder, "*.jpeg"));
        files.Sort(System.StringComparer.OrdinalIgnoreCase);

        foreach (var file in files)
        {
            string assetPath = file.Replace('\\', '/');

            var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer != null && !importer.isReadable)
            {
                importer.isReadable = true;
                importer.SaveAndReimport();
            }

            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
            if (sprite == null && texture != null)
                sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f));

            E_PixcelItem entry = GameObjectPool.Instance.Get<E_PixcelItem>(PoolType.PixcelItem, Vector3.zero, Quaternion.identity, content);
            if (entry == null)
                continue;

            Texture2D capturedTexture = texture;
            entry.Setup(sprite, Path.GetFileNameWithoutExtension(file), () => OnItemClicked(capturedTexture));

            spawnedItems.Add(entry);
        }
#else
        Debug.LogWarning("LoadPixcelsPopup only lists files inside the Unity Editor.");
#endif
    }

    private void OnItemClicked(Texture2D texture)
    {
        if (texture == null)
            return;

        if (E_BoxesManager.Instance != null)
            E_BoxesManager.Instance.DrawBoxes(texture);

        Hide();
    }
}
