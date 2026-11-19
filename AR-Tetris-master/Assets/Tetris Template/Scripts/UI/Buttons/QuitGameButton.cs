using UnityEngine;
using UnityEngine.SceneManagement;

// Home button shared by the 2D and AR menus: leaves the current game and
// shows play mode selection in the lobby (the 2D scene).
public class QuitGameButton : MonoBehaviour
{
    public const int LobbySceneIndex = 0;

    // Set before loading the lobby so it opens mode selection on arrival.
    private static bool openModeSelectionOnLoad;

    public static bool IsInLobby => SceneManager.GetActiveScene().buildIndex == LobbySceneIndex;

    public void OnClickQuitButton()
    {
        Managers.Audio.PlayUIClick();

        if (!IsInLobby)
        {
            openModeSelectionOnLoad = true;
            SceneManager.LoadScene(LobbySceneIndex);
            return;
        }

        Managers.Grid.ClearBoard();
        Managers.Score.ResetCurrentScore();
        Destroy(SpawnManager.next);
        SpawnManager.isFirst = true;
        Managers.Game.currentShape = null;
        Managers.Game.isGameActive = false;

        Managers.UI.MainMenuArrange();
        Managers.UI.popUps.ActivateDifficultyPopUp();
    }

    // Called by the lobby UI once it is ready.
    public static void OpenPendingModeSelection()
    {
        if (!openModeSelectionOnLoad)
            return;

        openModeSelectionOnLoad = false;
        Managers.UI.popUps.ActivateDifficultyPopUp();
    }
}
