
using UnityEngine;
using System.Collections;

public class GameBackButton : MonoBehaviour {

	public void OnClickBackButton()
	{
        Managers.Audio.PlayUIClick();
        Managers.Game.SetState(typeof(MenuState));
        if (SpawnManager.next != null)
            SpawnManager.next.SetActive(false);
        GameManager.difficultyTemp = GameManager.difficulty;
    }
}
