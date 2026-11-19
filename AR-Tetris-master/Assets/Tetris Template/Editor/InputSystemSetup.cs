using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;

// Android does not support "Both" input handling, so switch the project to the Input System only.
public static class InputSystemSetup
{
    private const int InputSystemOnly = 1; // 0 = legacy, 1 = Input System, 2 = both

    [MenuItem("AR Tetris/Use Input System Only")]
    public static void Configure()
    {
        foreach (string path in GameScenes.All)
            SceneEditing.Edit(path, ReplaceUIInputModules);

        bool changed = SetActiveInputHandler(InputSystemOnly);
        AssetDatabase.SaveAssets();
        Debug.Log("[Input] Project now uses the Input System only.");

        if (changed && !Application.isBatchMode &&
            EditorUtility.DisplayDialog("Restart required",
                "Active Input Handling changed to Input System. Restart Unity now?", "Restart", "Later"))
        {
            EditorApplication.OpenProject(System.IO.Directory.GetCurrentDirectory());
        }
    }

    public static bool IsNeeded()
    {
        return GetActiveInputHandler().intValue != InputSystemOnly;
    }

    // StandaloneInputModule reads the legacy Input Manager and throws without it.
    private static void ReplaceUIInputModules(Scene scene)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            foreach (StandaloneInputModule legacy in root.GetComponentsInChildren<StandaloneInputModule>(true))
            {
                GameObject go = legacy.gameObject;
                Object.DestroyImmediate(legacy);

                if (go.GetComponent<InputSystemUIInputModule>() == null)
                    go.AddComponent<InputSystemUIInputModule>().AssignDefaultActions();

                Debug.Log($"[Input] {scene.name}: replaced StandaloneInputModule on '{go.name}'");
            }
        }
    }

    private static bool SetActiveInputHandler(int value)
    {
        SerializedProperty property = GetActiveInputHandler();
        if (property.intValue == value)
            return false;

        property.intValue = value;
        property.serializedObject.ApplyModifiedProperties();
        return true;
    }

    private static SerializedProperty GetActiveInputHandler()
    {
        Object playerSettings = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0];
        return new SerializedObject(playerSettings).FindProperty("activeInputHandler");
    }
}
