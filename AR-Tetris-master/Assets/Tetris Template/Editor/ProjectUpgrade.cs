using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.XR.ARFoundation;

// One-shot migration from the Unity 2019 / AR Foundation 2 project to Unity 6.
// Run from menu "AR Tetris/Upgrade Project", or in batch mode with
// -executeMethod ProjectUpgrade.RunBatch
public static class ProjectUpgrade
{
    private const string ScenesFolder = "Assets/Tetris Template/Scenes/";
    private const string ARScenePath = ScenesFolder + "AR.unity";
    private const string GamePlayScenePath = ScenesFolder + "GamePlay.unity";
    private const string BlockMaterialPath = "Assets/Tetris Template/Resources/Block3DMaterial.mat";

    // Deprecated components, matched by full type name to avoid obsolete warnings.
    private const string LegacySessionOrigin = "UnityEngine.XR.ARFoundation.ARSessionOrigin";
    private const string LegacyPoseDriver = "UnityEngine.SpatialTracking.TrackedPoseDriver";

    // Online-only UI (login, SMS verify, leaderboards) removed for the offline build.
    private static readonly string[] OnlineUIPaths =
    {
        "UI/Popup/Login",
        "UI/Popup/Verify",
        "UI/Popup/Leaderboard",
        "UI/Popup/Leaderboard_Local",
        "UI/Popup/PlayerStats/Trash",
        "UI/Popup/PlayerStats/Local",
    };

    [MenuItem("AR Tetris/Upgrade Project")]
    public static void Run()
    {
        CreateBlockMaterial();
        UpgradeGamePlayScene();
        UpgradeARScene();
        MobileSetup.Configure();
        AssetDatabase.SaveAssets();
        Debug.Log("[Upgrade] Done.");
    }

    public static void RunBatch()
    {
        try
        {
            Run();
            EditorApplication.Exit(0);
        }
        catch (System.Exception e)
        {
            Debug.LogError("[Upgrade] Failed: " + e);
            EditorApplication.Exit(1);
        }
    }

    // ---------- 3D block material ----------

    [MenuItem("AR Tetris/Create Block Material")]
    public static void CreateBlockMaterial()
    {
        bool usesSrp = GraphicsSettings.defaultRenderPipeline != null;
        Shader shader = Shader.Find(usesSrp ? "Universal Render Pipeline/Lit" : "Standard");
        if (shader == null)
            throw new System.Exception("No lit shader found for the active render pipeline.");

        Material material = AssetDatabase.LoadAssetAtPath<Material>(BlockMaterialPath);
        if (material == null)
        {
            material = new Material(shader);
            AssetDatabase.CreateAsset(material, BlockMaterialPath);
        }

        material.shader = shader;
        material.enableInstancing = true;
        SetIfPresent(material, "_Glossiness", 0.6f);
        SetIfPresent(material, "_Smoothness", 0.6f);
        SetIfPresent(material, "_Metallic", 0.05f);
        EditorUtility.SetDirty(material);
        Debug.Log("[Upgrade] Block material uses shader " + shader.name);
    }

    private static void SetIfPresent(Material material, string property, float value)
    {
        if (material.HasProperty(property))
            material.SetFloat(property, value);
    }

    // ---------- Scenes ----------

    private static void UpgradeGamePlayScene()
    {
        Scene scene = EditorSceneManager.OpenScene(GamePlayScenePath, OpenSceneMode.Single);
        RemoveOnlineUI(scene);
        RemoveMissingScripts(scene);
        EditorSceneManager.SaveScene(scene);
    }

    private static void RemoveOnlineUI(Scene scene)
    {
        foreach (string path in OnlineUIPaths)
        {
            GameObject go = FindByPath(scene, path);
            if (go == null)
                continue;

            Object.DestroyImmediate(go);
            Debug.Log("[Upgrade] Removed online UI " + path);
        }
    }

    // Unlike GameObject.Find, this also finds inactive objects.
    private static GameObject FindByPath(Scene scene, string path)
    {
        int slash = path.IndexOf('/');
        string rootName = path.Substring(0, slash);

        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (root.name != rootName)
                continue;

            Transform found = root.transform.Find(path.Substring(slash + 1));
            if (found != null)
                return found.gameObject;
        }
        return null;
    }

    private static void UpgradeARScene()
    {
        Scene scene = EditorSceneManager.OpenScene(ARScenePath, OpenSceneMode.Single);
        RemoveMissingScripts(scene);

        GameObject originObject = FindInScene(scene, "AR Session Origin");
        Camera arCamera = FindInScene(scene, "AR Camera").GetComponent<Camera>();

        ReplaceSessionOrigin(originObject);
        RemoveComponentByTypeName(arCamera.gameObject, LegacyPoseDriver);

        XROrigin origin = SetupXROrigin(originObject, arCamera);
        SetupTrackedPoseDriver(arCamera.gameObject);
        EnsureComponent<ARInputManager>(FindInScene(scene, "AR Session"));
        LinkARController(scene, origin);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    // ARRaycastManager/ARPlaneManager require an origin, so detach them while swapping
    // the deprecated ARSessionOrigin for XROrigin, then restore their settings.
    private static void ReplaceSessionOrigin(GameObject originObject)
    {
        if (!HasComponentByTypeName(originObject, LegacySessionOrigin))
            return;

        string raycastJson = DetachComponent<ARRaycastManager>(originObject);
        string planeJson = DetachComponent<ARPlaneManager>(originObject);

        RemoveComponentByTypeName(originObject, LegacySessionOrigin);
        EnsureComponent<XROrigin>(originObject);

        RestoreComponent<ARRaycastManager>(originObject, raycastJson);
        RestoreComponent<ARPlaneManager>(originObject, planeJson);
    }

    private static string DetachComponent<T>(GameObject go) where T : Component
    {
        T component = go.GetComponent<T>();
        if (component == null)
            return null;

        string json = EditorJsonUtility.ToJson(component);
        Object.DestroyImmediate(component);
        return json;
    }

    private static void RestoreComponent<T>(GameObject go, string json) where T : Component
    {
        if (json == null)
            return;

        EditorJsonUtility.FromJsonOverwrite(json, go.AddComponent<T>());
    }

    private static XROrigin SetupXROrigin(GameObject originObject, Camera arCamera)
    {
        XROrigin origin = EnsureComponent<XROrigin>(originObject);
        origin.Origin = originObject;
        origin.Camera = arCamera;
        origin.CameraFloorOffsetObject = EnsureCameraOffset(originObject.transform, arCamera.transform).gameObject;
        origin.RequestedTrackingOriginMode = XROrigin.TrackingOriginMode.NotSpecified;
        EditorUtility.SetDirty(origin);
        Debug.Log("[Upgrade] XROrigin set up on " + originObject.name);
        return origin;
    }

    // XROrigin expects the camera under an offset object, not directly under the origin.
    private static Transform EnsureCameraOffset(Transform origin, Transform camera)
    {
        if (camera.parent != origin)
            return camera.parent;

        var offset = new GameObject("Camera Offset").transform;
        offset.SetParent(origin, false);
        camera.SetParent(offset, true);
        return offset;
    }

    private static void SetupTrackedPoseDriver(GameObject cameraObject)
    {
        if (cameraObject.GetComponent<TrackedPoseDriver>() != null)
            return;

        var driver = cameraObject.AddComponent<TrackedPoseDriver>();
        driver.positionInput = new InputActionProperty(CreateAction("Position", "Vector3",
            "<XRHMD>/centerEyePosition", "<HandheldARInputDevice>/devicePosition"));
        driver.rotationInput = new InputActionProperty(CreateAction("Rotation", "Quaternion",
            "<XRHMD>/centerEyeRotation", "<HandheldARInputDevice>/deviceRotation"));
        EditorUtility.SetDirty(driver);
        Debug.Log("[Upgrade] TrackedPoseDriver (Input System) added to " + cameraObject.name);
    }

    private static InputAction CreateAction(string name, string controlType, params string[] bindings)
    {
        var action = new InputAction(name, InputActionType.Value, expectedControlType: controlType);
        foreach (string binding in bindings)
            action.AddBinding(binding);
        return action;
    }

    private static void LinkARController(Scene scene, XROrigin origin)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            foreach (ARController controller in root.GetComponentsInChildren<ARController>(true))
            {
                controller.XROrigin = origin;
                controller.ARRaycastManager = origin.GetComponent<ARRaycastManager>();
                controller.ARPlaneManager = origin.GetComponent<ARPlaneManager>();
                EditorUtility.SetDirty(controller);
            }
        }
    }

    // ---------- Helpers ----------

    private static void RemoveMissingScripts(Scene scene)
    {
        foreach (GameObject go in AllObjects(scene))
        {
            int removed = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
            if (removed > 0)
                Debug.Log($"[Upgrade] {scene.name}: removed {removed} missing script(s) on '{go.name}'");
        }
    }

    private static bool HasComponentByTypeName(GameObject go, string fullTypeName)
    {
        foreach (Component component in go.GetComponents<Component>())
            if (component != null && component.GetType().FullName == fullTypeName)
                return true;
        return false;
    }

    private static void RemoveComponentByTypeName(GameObject go, string fullTypeName)
    {
        foreach (Component component in go.GetComponents<Component>())
        {
            if (component != null && component.GetType().FullName == fullTypeName)
                Object.DestroyImmediate(component);
        }

        if (HasComponentByTypeName(go, fullTypeName))
            throw new System.Exception($"Could not remove {fullTypeName} from '{go.name}'");

        Debug.Log($"[Upgrade] Removed {fullTypeName} from '{go.name}'");
    }

    private static IEnumerable<GameObject> AllObjects(Scene scene)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
            foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
                yield return t.gameObject;
    }

    private static GameObject FindInScene(Scene scene, string name)
    {
        foreach (GameObject go in AllObjects(scene))
            if (go.name == name)
                return go;

        throw new System.Exception($"'{name}' not found in scene {scene.name}");
    }

    private static T EnsureComponent<T>(GameObject go) where T : Component
    {
        T component = go.GetComponent<T>();
        return component != null ? component : go.AddComponent<T>();
    }
}
