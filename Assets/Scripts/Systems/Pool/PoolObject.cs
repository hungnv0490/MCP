using UnityEngine;


public class PoolObject : MonoBehaviour
{
    public PoolType PoolType;

    [HideInInspector]
    public bool IsPooled;
}