
using UnityEngine;
using System.Collections;

public class CameraManager : MonoBehaviour {

	public Camera main;

    private float _mainMenuSize = 13.5f;
    private float _inGameSize = 11f;

    [HideInInspector]
	public CameraShake shaker;

	void Awake()
	{
		shaker = main.gameObject.GetComponent<CameraShake> ();
	}

    public void ZoomIn()
    {
        if (main.orthographicSize != _inGameSize)
        {
            SimpleTween.OrthoSize(main, _inGameSize, 1f, () => StartCoroutine(StartGamePlay()));
        }
        else
        {
            Managers.Spawner.Spawn();
            Managers.Game.isGameActive = true;
        }
    }

    public void ZoomOut()
    {
        if (main.orthographicSize != _mainMenuSize)
            SimpleTween.OrthoSize(main, _mainMenuSize, 1f);
    }

    IEnumerator StartGamePlay()
    {
        yield return new WaitForEndOfFrame();

        if (!Managers.Game.isGameActive)
        {
            Managers.Spawner.Spawn();
            Managers.Game.isGameActive = true;
        }
        yield break;
    }



}
