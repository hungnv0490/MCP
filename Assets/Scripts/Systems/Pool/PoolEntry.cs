using UnityEngine;

[System.Serializable]
public struct PoolEntry
{
    public PoolType poolKey;
    public Component prefab;
    public int preloadCount;
}

