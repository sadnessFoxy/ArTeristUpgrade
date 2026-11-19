using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Scene-level UI fixes: drops leftover social/credit links, keeps menus above the 3D blocks
// and gives both scenes the same home button back to play mode selection.
public static class SceneUIFixes
{
    // Blocks sit around z = 0 and the camera at z = -10, so 1 unit in front of the camera
    // keeps the UI plane in front of every block.
    private const float UIPlaneDistance = 1f;

    private const string AtlasPath = "Assets/Tetris Template/Sprites/SkardFlatAtlas.png";
    private const string HomeIconName = "SkardFlatAtlas_13";
    private static readonly Color HomeButtonColor = new Color(0.27f, 0.56f, 0.93f);

    private static readonly string[] LeftoverLinks =
    {
        "Popup/Settings/Credits",
        "Popup/Settings/Rate",
        "Popup/Settings/Twitter",
        "Popup/Settings/Facebook",
    };

    [MenuItem("AR Tetris/Apply UI Fixes")]
    public static void Run()
    {
        SceneEditing.Edit(GameScenes.GamePlay, scene => FixScene(scene, isARScene: false));
        SceneEditing.Edit(GameScenes.AR, scene => FixScene(scene, isARScene: true));
        Debug.Log("[UI Fixes] Done.");
    }

    // Cheap text check so it can run on every editor load.
    public static bool IsNeeded()
    {
        string gamePlay = File.ReadAllText(GameScenes.GamePlay);
        string ar = File.ReadAllText(GameScenes.AR);
        return HasObjectNamed(gamePlay, "Rate") || !HasObjectNamed(gamePlay, "Quit") ||
               HasObjectNamed(ar, "Leave") || !HasObjectNamed(ar, "Quit");
    }

    private static bool HasObjectNamed(string sceneText, string name)
    {
        return Regex.IsMatch(sceneText, @"^\s*m_Name: " + name + @"\r?$", RegexOptions.Multiline);
    }

    private static void FixScene(Scene scene, bool isARScene)
    {
        Transform ui = FindRoot(scene, "UI");

        RemoveLeftoverLinks(ui);
        CenterSoundButton(ui);

        if (!isARScene)
            KeepUIAboveBlocks(ui);

        SetUpHomeButton(ui);
    }

    private static void RemoveLeftoverLinks(Transform ui)
    {
        foreach (string path in LeftoverLinks)
        {
            Transform link = ui.Find(path);
            if (link == null)
                continue;

            Object.DestroyImmediate(link.gameObject);
            Debug.Log("[UI Fixes] Removed " + path);
        }
    }

    // With the links gone, the sound toggle is the only settings button left.
    private static void CenterSoundButton(Transform ui)
    {
        var sound = ui.Find("Popup/Settings/Sound") as RectTransform;
        if (sound != null)
            sound.anchoredPosition = new Vector2(0f, sound.anchoredPosition.y);
    }

    private static void KeepUIAboveBlocks(Transform ui)
    {
        Canvas canvas = ui.GetComponent<Canvas>();
        if (canvas.renderMode != RenderMode.ScreenSpaceCamera)
            return;

        canvas.planeDistance = UIPlaneDistance;
        EditorUtility.SetDirty(canvas);
    }

    // Both scenes get the same home button: cloned from Restart for the menu style,
    // with the home icon and its own colour. The AR scene's old "Leave" button is replaced.
    private static void SetUpHomeButton(Transform ui)
    {
        MainMenu menu = ui.GetComponentInChildren<MainMenu>(true);
        if (menu.restartButton == null)
            menu.restartButton = ui.Find("Menu_UI/Restart").gameObject;

        Transform leave = ui.Find("Menu_UI/Leave");
        if (leave != null)
            Object.DestroyImmediate(leave.gameObject);

        GameObject home = menu.quitButton != null ? menu.quitButton : CloneRestartButton(menu.restartButton);
        WireHomeButton(home);
        StyleHomeButton(home);

        menu.quitButton = home;
        EditorUtility.SetDirty(menu);
        Debug.Log($"[UI Fixes] Home button set up in {ui.gameObject.scene.name}");
    }

    private static GameObject CloneRestartButton(GameObject restart)
    {
        GameObject home = Object.Instantiate(restart, restart.transform.parent);
        home.name = "Quit";
        home.transform.SetSiblingIndex(restart.transform.GetSiblingIndex() + 1);
        Object.DestroyImmediate(home.GetComponent<RestartButton>());
        return home;
    }

    private static void WireHomeButton(GameObject home)
    {
        QuitGameButton handler = home.GetComponent<QuitGameButton>();
        if (handler == null)
            handler = home.AddComponent<QuitGameButton>();

        Button button = home.GetComponent<Button>();
        while (button.onClick.GetPersistentEventCount() > 0)
            UnityEventTools.RemovePersistentListener(button.onClick, 0);
        UnityEventTools.AddPersistentListener(button.onClick, handler.OnClickQuitButton);
    }

    private static void StyleHomeButton(GameObject home)
    {
        home.GetComponent<Image>().color = HomeButtonColor;

        Image icon = home.transform.Find("Image").GetComponent<Image>();
        icon.sprite = LoadAtlasSprite(HomeIconName);
        icon.transform.localRotation = Quaternion.Euler(0f, 0f, 180f); // the atlas stores it upside down
    }

    private static Sprite LoadAtlasSprite(string name)
    {
        foreach (Object asset in AssetDatabase.LoadAllAssetsAtPath(AtlasPath))
            if (asset is Sprite sprite && sprite.name == name)
                return sprite;

        throw new System.Exception($"Sprite '{name}' not found in {AtlasPath}");
    }

    private static Transform FindRoot(Scene scene, string name)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
            if (root.name == name)
                return root.transform;

        throw new System.Exception($"'{name}' not found in scene {scene.name}");
    }
}
