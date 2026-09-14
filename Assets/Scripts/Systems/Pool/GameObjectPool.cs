using System.Collections.Generic;
using UnityEngine;

public class GameObjectPool : MonoBehaviour
{
    private class PoolData
    {
        public Component Prefab;

        public readonly Queue<Component> Queue = new();
        public readonly HashSet<Component> ActiveSet = new();

        public int TotalCreated;
        public int MaxCapacity;
    }

    public static GameObjectPool Instance { get; private set; }

    [SerializeField] private List<PoolEntry> entries;

    [Header("Pool Settings")]
    [SerializeField] private int defaultMaxCapacity = 500;

    private readonly Dictionary<PoolType, PoolData> _poolMap = new();

    #region Unity

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);

        InitializePools();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    #endregion

    #region Init

    private void InitializePools()
    {
        foreach (var entry in entries)
        {
            if (entry.prefab == null)
                continue;

            PoolData data = new()
            {
                Prefab = entry.prefab,
                MaxCapacity = defaultMaxCapacity
            };

            _poolMap[entry.poolKey] = data;

            for (int i = 0; i < entry.preloadCount; i++)
            {
                Component item = CreateNewItem(entry.poolKey, data);

                if (item == null)
                    break;

                ReturnToQueue(item, data);
            }
        }
    }

    #endregion

    #region Get

    public T Get<T>(
        PoolType key,
        Vector3 position,
        Quaternion rotation = default,
        Transform parent = null)
        where T : Component
    {
        if (!_poolMap.TryGetValue(key, out PoolData data))
        {
            Debug.LogError($"[Pool] Missing PoolType: {key}");
            return null;
        }

        if (rotation == default) rotation = Quaternion.identity;

        Component item = GetValidObject(data);

        if (item == null)
        {
            item = CreateNewItem(key, data);

            if (item == null)
                return null;
        }

        T result = item.GetComponent<T>();

        if (result == null)
        {
            Debug.LogError(
                $"[Pool] PoolType {key} does not contain component {typeof(T).Name}");

            ReturnToQueue(item, data);
            return null;
        }

        PoolObject poolObject = item.GetComponent<PoolObject>();

        poolObject.IsPooled = false;

        data.ActiveSet.Add(item);

        item.transform.SetParent(parent, false);
        item.transform.SetPositionAndRotation(position, rotation);

        item.gameObject.SetActive(true);

        if (item.TryGetComponent<IPoolable>(out var poolable))
        {
            poolable.OnSpawn();
        }

        return result;
    }

    #endregion

    #region Return

    public void ReturnToPool(Component item)
    {
        if (item == null)
            return;

        if (!item.TryGetComponent<PoolObject>(out var poolObject))
        {
            Destroy(item.gameObject);
            return;
        }

        if (!_poolMap.TryGetValue(poolObject.PoolType, out PoolData data))
        {
            Destroy(item.gameObject);
            return;
        }

        if (poolObject.IsPooled)
            return;

        if (item.TryGetComponent<IPoolable>(out var poolable))
        {
            poolable.OnDespawn();
        }

        data.ActiveSet.Remove(item);

        ReturnToQueue(item, data);
    }

    private void ReturnToQueue(Component item, PoolData data)
    {
        if (item == null)
            return;

        PoolObject poolObject = item.GetComponent<PoolObject>();

        poolObject.IsPooled = true;

        item.transform.SetParent(transform, false);

        item.gameObject.SetActive(false);

        data.Queue.Enqueue(item);
    }

    #endregion

    #region Internal

    private Component GetValidObject(PoolData data)
    {
        while (data.Queue.Count > 0)
        {
            Component item = data.Queue.Dequeue();

            if (item != null)
                return item;

            data.TotalCreated--;
        }

        return null;
    }

    private Component CreateNewItem(
        PoolType key,
        PoolData data)
    {
        if (data.TotalCreated >= data.MaxCapacity)
        {
            Debug.LogWarning(
                $"[Pool] Pool {key} reached Max Capacity ({data.MaxCapacity})");

            return null;
        }

        Component item =
            Instantiate(data.Prefab, transform, false);

        PoolObject poolObject =
            item.GetComponent<PoolObject>();

        if (poolObject == null)
        {
            Debug.LogError($"[Pool] Prefab {data.Prefab.name} thiếu PoolObject component!");
            Destroy(item.gameObject);
            return null;
        }

        poolObject.PoolType = key;
        poolObject.IsPooled = true;

        item.gameObject.SetActive(false);

        data.TotalCreated++;

        return item;
    }

    #endregion

    #region Reset

    public void ResetAll()
    {
        foreach (var pair in _poolMap)
        {
            PoolData data = pair.Value;

            if (data.ActiveSet.Count == 0)
                continue;

            Component[] snapshot =
                new Component[data.ActiveSet.Count];

            data.ActiveSet.CopyTo(snapshot);

            foreach (Component item in snapshot)
            {
                if (item == null)
                    continue;

                ReturnToPool(item);
            }

            data.ActiveSet.Clear();
        }
    }

    #endregion

    #region Debug

    public int GetActiveCount(PoolType key)
    {
        if (!_poolMap.TryGetValue(key, out PoolData data))
            return 0;

        return data.ActiveSet.Count;
    }

    public int GetAvailableCount(PoolType key)
    {
        if (!_poolMap.TryGetValue(key, out PoolData data))
            return 0;

        return data.Queue.Count;
    }

    #endregion
}