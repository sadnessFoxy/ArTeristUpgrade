using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static int difficulty = 0; // 0 for easy, 1 for hard
    public static int difficultyTemp = 0;

    public bool isGameActive;
    public TetrisShape currentShape;
    public Transform blockHolder;
    public PlayerStats stats;

    private _StatesBase currentState;
    public _StatesBase State
    {
        get { return currentState; }
    }

    void Awake()
    {
        // Mobile defaults to 30 fps, which makes touch controls feel sluggish.
        Application.targetFrameRate = 60;
        isGameActive = false;
        LoadStats();
    }

    void Start()
    {
        SetState(typeof(MenuState));
    }

    void Update()
    {
        Time.timeScale = (difficulty == 1) ? 2 : 1;

        if (currentState != null)
            currentState.OnUpdate();
    }

    // Changes the current game state
    public void SetState(System.Type newStateType)
    {
        if (currentState != null)
            currentState.OnDeactivate();

        currentState = GetComponentInChildren(newStateType) as _StatesBase;
        if (currentState != null)
            currentState.OnActivate();
    }

    public void SaveStats()
    {
        StatsStorage.Save(stats);
    }

    // Work on a runtime copy so play sessions never overwrite the asset.
    private void LoadStats()
    {
        stats = (stats != null) ? Instantiate(stats) : ScriptableObject.CreateInstance<PlayerStats>();
        stats.ClearStats();
        StatsStorage.Load(stats);
    }

    void OnApplicationPause(bool paused)
    {
        if (paused)
            SaveStats();
    }

    void OnApplicationQuit()
    {
        SaveStats();
    }
}
