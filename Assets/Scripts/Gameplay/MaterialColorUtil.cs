using UnityEngine;

public static class MaterialColorUtil
{
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorId = Shader.PropertyToID("_Color");

    public static void Apply(Renderer renderer, Color color)
    {
        // SpriteRenderer has a dedicated per-instance tint that doesn't allocate a
        // material instance at all -- cheaper and simpler than going through material.
        if (renderer is SpriteRenderer spriteRenderer)
        {
            spriteRenderer.color = color;
            return;
        }

        Material material = renderer.material;

        if (material.HasProperty(BaseColorId))
            material.SetColor(BaseColorId, color);
        else if (material.HasProperty(ColorId))
            material.SetColor(ColorId, color);
    }
}
