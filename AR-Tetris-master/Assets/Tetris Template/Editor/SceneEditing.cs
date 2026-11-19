using System;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

// Edits a scene asset without replacing whatever scene the user has open.
public static class SceneEditing
{
    public static void Edit(string scenePath, Action<Scene> edit)
    {
        Scene scene = SceneManager.GetSceneByPath(scenePath);
        bool wasLoaded = scene.isLoaded;
        if (!wasLoaded)
            scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);

        edit(scene);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        if (!wasLoaded)
            EditorSceneManager.CloseScene(scene, true);
    }
}
