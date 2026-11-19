using UnityEngine;

public class SettingsMenu : MonoBehaviour
{
    public GameObject soundCross;
    public GameObject soundOnText;
    public GameObject soundOffText;

    public void TurnUpDownSound()
    {
        bool turnOn = AudioListener.volume == 0f;

        AudioListener.volume = turnOn ? 1f : 0f;
        soundCross.SetActive(!turnOn);
        soundOffText.SetActive(!turnOn);
        soundOnText.SetActive(turnOn);

        if (turnOn)
            Managers.Audio.PlayUIClick();
    }
}
