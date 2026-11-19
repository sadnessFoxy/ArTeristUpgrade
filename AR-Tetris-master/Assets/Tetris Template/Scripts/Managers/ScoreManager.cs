using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public int currentScore = 0;
    public int highScore;

    // Start (not Awake) so GameManager has already loaded saved stats.
    void Start()
    {
        highScore = Managers.Game.stats.highScore;
        Managers.UI.inGameUI.UpdateScoreUI();
    }

    public void OnScore(int scoreIncreaseAmount)
    {
        currentScore += scoreIncreaseAmount;
        Managers.Game.stats.totalScore += scoreIncreaseAmount;
        CheckHighScore();
        Managers.UI.inGameUI.UpdateScoreUI();
    }

    public void CheckHighScore()
    {
        if (Managers.Game.stats.TrySetHighScore(currentScore))
            Managers.Game.SaveStats();

        highScore = Managers.Game.stats.highScore;
    }

    // Merges a score from another source (e.g. the server) and keeps the best one.
    public void MergeHighScore(int score)
    {
        Managers.Game.stats.TrySetHighScore(score);
        Managers.Game.SaveStats();
        highScore = Managers.Game.stats.highScore;
        Managers.UI.inGameUI.UpdateScoreUI();
    }

    public void ResetScore()
    {
        currentScore = 0;
        Managers.UI.inGameUI.UpdateScoreUI();
    }

    public void ResetCurrentScore()
    {
        ResetScore();
    }
}
