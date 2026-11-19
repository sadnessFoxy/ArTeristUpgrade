using UnityEngine;

// The original 2D scenes have no lights; 3D blocks need one to show their shape.
public static class SceneLighting
{
    private static readonly Vector3 KeyLightAngles = new Vector3(35f, -30f, 0f);
    private static Light keyLight;

    public static void EnsureKeyLight()
    {
        if (keyLight != null)
            return;

        keyLight = FindDirectionalLight();
        if (keyLight != null)
            return;

        var go = new GameObject("Block Key Light");
        go.transform.rotation = Quaternion.Euler(KeyLightAngles);

        keyLight = go.AddComponent<Light>();
        keyLight.type = LightType.Directional;
        keyLight.intensity = 0.85f;
        keyLight.shadows = LightShadows.None;

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.42f, 0.42f, 0.46f);
    }

    private static Light FindDirectionalLight()
    {
        foreach (Light light in Object.FindObjectsByType<Light>())
        {
            if (light.type == LightType.Directional && light.isActiveAndEnabled)
                return light;
        }
        return null;
    }
}
