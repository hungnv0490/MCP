// using System.Collections.Generic;
// using UnityEngine;

// public static class PoolExtensions
// {
//     private static readonly Dictionary<BoxType, PoolType> _boxMap = new()
//     {
//         { BoxType.Normal, PoolType.NormalBox },
//         { BoxType.Mystery, PoolType.MysteryBox },
//         { BoxType.KeyBox, PoolType.KeyBox },
//         { BoxType.Connected, PoolType.ConnectedBox }
//     };

//     public static PoolType ToPoolType(this BoxType type) => _boxMap.GetValueOrDefault(type, PoolType.None);
// }