
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class StatsUI : MonoBehaviour {

    public Text highScore;
    public Text timeSpent;
    public Text numberOfGames;

    public void ClearStats()
    {
        Managers.Game.stats.ClearStats();
        Managers.Game.SaveStats();
        Managers.Score.highScore = 0;
        Managers.UI.inGameUI.UpdateScoreUI();
        RefreshText();
    }

    void OnEnable()
    {
        RefreshText();
    }

    void RefreshText()
    {
        PlayerStats stats = Managers.Game.stats;
        SetText(highScore, stats.highScore.ToString());
        SetText(timeSpent, TimeUtil.SecondsToHMS(stats.timeSpent));
        SetText(numberOfGames, stats.numberOfGames.ToString());
    }

    // Some popups reuse this component without every label assigned.
    private static void SetText(Text label, string value)
    {
        if (label != null)
            label.text = value;
    }

}
