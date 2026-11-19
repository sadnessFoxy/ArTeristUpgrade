
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public enum Menus
{
	MAIN,
	INGAME,
	GAMEOVER
}

public class UIManager : MonoBehaviour {

	public MainMenu mainMenu;
	public InGameUI inGameUI;
    public PopUp popUps;
    public GameObject activePopUp;
    public GameObject panel;

	public void ActivateUI(Menus menutype)
	{
		if (menutype.Equals (Menus.MAIN))
		{
            StartCoroutine(ActivateMainMenu());          
		}
		else if(menutype.Equals(Menus.INGAME))
		{
            StartCoroutine(ActivateInGameUI());
		}	
	}
    
    IEnumerator ActivateMainMenu()
    {
        inGameUI.InGameUIEndAnimation();
        inGameUI.gameObject.SetActive(false);
        yield return new WaitForSeconds(0.3f);
        mainMenu.gameObject.SetActive(true);
        mainMenu.MainMenuStartAnimation();
    }

    IEnumerator ActivateInGameUI()
    {
        mainMenu.MainMenuEndAnimation();       
        yield return new WaitForSeconds(0.3f);
        mainMenu.gameObject.SetActive(false);
        popUps.gameObject.SetActive(false);
        inGameUI.gameObject.SetActive(true);
        inGameUI.InGameUIStartAnimation();
    }

    void Start()
    {
        QuitGameButton.OpenPendingModeSelection();
    }

    void Update()
    {
        if (activePopUp != null)
            HideIfClickedOutside(activePopUp);
    }

    public void MainMenuArrange()
    {
        bool inGame = Managers.Game.isGameActive;
        mainMenu.layout.spacing = inGame ? 20 : 50;
        mainMenu.layout.padding.left = mainMenu.layout.padding.right = inGame ? 120 : 250;

        // Restart only makes sense mid-game. Home is also the way back from the AR scene.
        SetActiveIfPresent(mainMenu.restartButton, inGame);
        SetActiveIfPresent(mainMenu.quitButton, inGame || !QuitGameButton.IsInLobby);
    }

    private static void SetActiveIfPresent(GameObject go, bool active)
    {
        if (go != null)
            go.SetActive(active);
    }

    private void HideIfClickedOutside(GameObject outsidePanel)
    {
        Pointer pointer = Pointer.current;
        if (pointer != null && pointer.press.isPressed && outsidePanel.activeSelf &&
            !RectTransformUtility.RectangleContainsScreenPoint(
                outsidePanel.GetComponent<RectTransform>(),
                pointer.position.ReadValue(),
                Camera.main))
        {
            outsidePanel.SetActive(false);
            outsidePanel.transform.parent.gameObject.SetActive(false);
            Managers.UI.panel.SetActive(false);
            activePopUp = null;
        }
    }

}
