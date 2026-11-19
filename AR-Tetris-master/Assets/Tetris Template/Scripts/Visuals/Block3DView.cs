using UnityEngine;

// Swaps the flat sprite of each block for a lit 3D rounded cube.
public static class Block3DView
{
    private const string MaterialResource = "Block3DMaterial";
    private static readonly int ColorId = Shader.PropertyToID("_Color");
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

    private static Material sharedMaterial;
    private static MaterialPropertyBlock propertyBlock;

    public static void Apply(Transform shape, Color color)
    {
        SceneLighting.EnsureKeyLight();

        foreach (SpriteRenderer sprite in shape.GetComponentsInChildren<SpriteRenderer>(true))
        {
            GameObject block = sprite.gameObject;
            Object.DestroyImmediate(sprite);
            AddCube(block, color);
        }
    }

    // Recolors blocks that were already converted.
    public static void SetColor(Transform shape, Color color)
    {
        foreach (MeshRenderer renderer in shape.GetComponentsInChildren<MeshRenderer>(true))
            ApplyColor(renderer, color);
    }

    private static void AddCube(GameObject block, Color color)
    {
        block.AddComponent<MeshFilter>().sharedMesh = RoundedCubeMesh.Shared;

        var renderer = block.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = GetMaterial();
        ApplyColor(renderer, color);
    }

    private static void ApplyColor(Renderer renderer, Color color)
    {
        if (propertyBlock == null)
            propertyBlock = new MaterialPropertyBlock();

        renderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetColor(ColorId, color);
        propertyBlock.SetColor(BaseColorId, color);
        renderer.SetPropertyBlock(propertyBlock);
    }

    private static Material GetMaterial()
    {
        if (sharedMaterial != null)
            return sharedMaterial;

        sharedMaterial = Resources.Load<Material>(MaterialResource);
        if (sharedMaterial == null)
        {
            Debug.LogWarning("Block3DMaterial not found in Resources, falling back to a runtime material.");
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
                shader = Shader.Find("Standard");
            sharedMaterial = new Material(shader);
        }
        return sharedMaterial;
    }
}
