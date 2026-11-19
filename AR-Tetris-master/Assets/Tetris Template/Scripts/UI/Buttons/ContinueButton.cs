using UnityEngine;

public class ContinueButton : MonoBehaviour
{
    public ARController AR;

    public void OnClickPlayButton()
    {
        Managers.Audio.PlayUIClick();

        // No active game yet: let the player pick a difficulty first.
        if (Managers.Game.blockHolder.childCount == 0)
            Managers.UI.popUps.ActivateDifficultyPopUp();
        else
            Managers.Game.SetState(typeof(GamePlayState));
    }

    public void OnClickContinueButton()
    {
        StartWithDifficulty(0);
    }

    public void OnClickContinueHardButton()
    {
        StartWithDifficulty(1);
    }

    public void AROnClickContinueButton()
    {
        if (AR.ARObject != null)
        {
            Managers.Audio.PlayUIClick();
            Managers.Game.SetState(typeof(GamePlayState));
        }
        else
        {
            AR.OnClick_PlaceObject();
        }
    }

    private void StartWithDifficulty(int difficulty)
    {
        Managers.UI.popUps.DeactivateDifficultyPopUp();
        Managers.Audio.PlayUIClick();
        GameManager.difficulty = difficulty;
        Managers.Game.SetState(typeof(GamePlayState));

        // On a fresh game the preview is created only after the camera zoom finishes.
        if (SpawnManager.next != null)
            SpawnManager.next.SetActive(true);
    }
}
