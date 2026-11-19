using UnityEngine;

// Persists PlayerStats (high score, play time, game count) across app restarts.
public static class StatsStorage
{
    private const string Key = "ARTetris.PlayerStats.v1";

    public static void Load(PlayerStats target)
    {
        if (!PlayerPrefs.HasKey(Key))
            return;

        try
        {
            JsonUtility.FromJsonOverwrite(PlayerPrefs.GetString(Key), target);
        }
        catch (System.ArgumentException e)
        {
            Debug.LogWarning("Saved stats are corrupted, starting fresh: " + e.Message);
            target.ClearStats();
        }
    }

    public static void Save(PlayerStats source)
    {
        PlayerPrefs.SetString(Key, JsonUtility.ToJson(source));
        PlayerPrefs.Save();
    }
}
