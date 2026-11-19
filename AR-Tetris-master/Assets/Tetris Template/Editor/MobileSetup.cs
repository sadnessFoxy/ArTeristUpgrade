using UnityEditor;
using UnityEditor.Build;
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;
using UnityEngine;
using UnityEngine.XR.Management;

// Player and XR settings required to run on Android (ARCore) and iOS (ARKit).
public static class MobileSetup
{
    private const string XRSettingsPath = "Assets/XR/XRGeneralSettingsPerBuildTarget.asset";
    private const string ARCoreLoader = "UnityEngine.XR.ARCore.ARCoreLoader";
    private const string ARKitLoader = "UnityEngine.XR.ARKit.ARKitLoader";
    private const string CameraUsage = "AR Tetris uses the camera to place the game board in augmented reality.";

    [MenuItem("AR Tetris/Configure Mobile Build")]
    public static void Configure()
    {
        ConfigureAndroid();
        ConfigureIOS();
        EnableARLoaders();
        AssetDatabase.SaveAssets();
        Debug.Log("[Upgrade] Mobile build settings configured.");
    }

    private static void ConfigureAndroid()
    {
        // ARCore and Google Play both require 64-bit IL2CPP builds.
        PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        if (PlayerSettings.Android.minSdkVersion < AndroidSdkVersions.AndroidApiLevel26)
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
    }

    private static void ConfigureIOS()
    {
        PlayerSettings.iOS.cameraUsageDescription = CameraUsage;
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.iOS, "com.group29.ARTetris");
    }

    private static void EnableARLoaders()
    {
        XRGeneralSettingsPerBuildTarget perTarget = GetOrCreateXRSettings();
        AssignLoader(perTarget, BuildTargetGroup.Android, ARCoreLoader);
        AssignLoader(perTarget, BuildTargetGroup.iOS, ARKitLoader);
    }

    private static void AssignLoader(XRGeneralSettingsPerBuildTarget perTarget, BuildTargetGroup group, string loader)
    {
        if (!perTarget.HasManagerSettingsForBuildTarget(group))
            perTarget.CreateDefaultManagerSettingsForBuildTarget(group);

        XRGeneralSettings settings = perTarget.SettingsForBuildTarget(group);
        settings.InitManagerOnStart = true;

        if (XRPackageMetadataStore.AssignLoader(settings.Manager, loader, group))
            Debug.Log($"[Upgrade] XR loader {loader} enabled for {group}");
        EditorUtility.SetDirty(settings);
    }

    private static XRGeneralSettingsPerBuildTarget GetOrCreateXRSettings()
    {
        if (EditorBuildSettings.TryGetConfigObject(XRGeneralSettings.k_SettingsKey, out XRGeneralSettingsPerBuildTarget existing)
            && existing != null)
            return existing;

        if (!AssetDatabase.IsValidFolder("Assets/XR"))
            AssetDatabase.CreateFolder("Assets", "XR");

        var created = ScriptableObject.CreateInstance<XRGeneralSettingsPerBuildTarget>();
        AssetDatabase.CreateAsset(created, XRSettingsPath);
        EditorBuildSettings.AddConfigObject(XRGeneralSettings.k_SettingsKey, created, true);
        return created;
    }
}
