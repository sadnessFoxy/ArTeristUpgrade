using UnityEngine;

public class PopUp : MonoBehaviour
{
    public GameObject gameOverPopUp;
    public GameObject settingsPopUp;
    public GameObject playerStatsPopUp;
    public GameObject difficultyPopUp;
    public GameObject ARinfoPopUp;

    public void ActivateGameOverPopUp() => Show(gameOverPopUp);
    public void ActivateSettingsPopUp() => Show(settingsPopUp);
    public void ActivatePlayerStatsPopUp() => Show(playerStatsPopUp);
    public void ActivateDifficultyPopUp() => Show(difficultyPopUp);
    public void ARInfoButtonPopUp() => Show(ARinfoPopUp);

    public void DeactivatePlayerStatsPopUp() => Hide(playerStatsPopUp);
    public void DeactivateDifficultyPopUp() => Hide(difficultyPopUp);

    private static void Show(GameObject popUp)
    {
        popUp.transform.parent.gameObject.SetActive(true);
        popUp.SetActive(true);
        Managers.UI.activePopUp = popUp;
    }

    private static void Hide(GameObject popUp)
    {
        popUp.transform.parent.gameObject.SetActive(false);
        popUp.SetActive(false);
    }
}
