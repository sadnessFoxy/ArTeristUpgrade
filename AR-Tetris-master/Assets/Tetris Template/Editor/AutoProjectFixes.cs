using UnityEditor;
using UnityEngine;

// Applies pending scene/project fixes after scripts compile, so they don't depend on
// someone remembering to run the "AR Tetris" menu items.
[InitializeOnLoad]
public static class AutoProjectFixes
{
    static AutoProjectFixes()
    {
        if (Application.isBatchMode)
            return;

        EditorApplication.delayCall += ApplyPending;
    }

    private static void ApplyPending()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            return;

        if (SceneUIFixes.IsNeeded())
            SceneUIFixes.Run();

        // Last, because it may restart the editor.
        if (InputSystemSetup.IsNeeded())
            InputSystemSetup.Configure();
    }
}
