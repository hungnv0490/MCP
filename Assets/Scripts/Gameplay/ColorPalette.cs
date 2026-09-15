using System.Collections.Generic;
using UnityEngine;

public static class ColorPalette
{
    private static readonly Dictionary<BallColorType, Color> _colors = new()
    {
        { BallColorType.Red, new Color(0.85f, 0.24f, 0.24f) },
        { BallColorType.Blue, new Color(0.25f, 0.47f, 0.85f) },
        { BallColorType.Green, new Color(0.32f, 0.72f, 0.35f) },
        { BallColorType.Yellow, new Color(0.95f, 0.8f, 0.22f) },
        { BallColorType.Purple, new Color(0.6f, 0.35f, 0.82f) },
    };

    private static readonly BallColorType[] _values =
        (BallColorType[])System.Enum.GetValues(typeof(BallColorType));

    public static Color Get(BallColorType type) =>
        _colors.TryGetValue(type, out Color color) ? color : Color.white;

    public static BallColorType GetRandom() =>
        _values[Random.Range(0, _values.Length)];
}
