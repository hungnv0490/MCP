using UnityEngine;

public static class MaterialColorUtil
{
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorId = Shader.PropertyToID("_Color");

    public static void Apply(Renderer renderer, Color color)
    {
        Material material = renderer.material;

        if (material.HasProperty(BaseColorId))
            material.SetColor(BaseColorId, color);
        else if (material.HasProperty(ColorId))
            material.SetColor(ColorId, color);
    }
}
