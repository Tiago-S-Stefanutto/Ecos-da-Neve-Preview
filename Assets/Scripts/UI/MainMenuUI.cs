using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenuUI : MonoBehaviour
{
    [Header("Menus")]
    public GameObject mainScreen;
    public GameObject optionsScreen;

    [Header("Sliders")]
    public Slider masterSlider;
    public Slider musicSlider;
    public Slider sfxSlider;

    public void StartGame()
    {
        AudioManager.Instance.PlaySFX(AudioManager.Instance.menuClickSound);
        GameManager.Instance.LoadScene("DemoLevel");
    }

    public void OpenOptions()
    {
        AudioManager.Instance.PlaySFX(AudioManager.Instance.menuSelectSound);
        mainScreen.SetActive(false);
        optionsScreen.SetActive(true);
    }

    public void CloseOptions()
    {
        AudioManager.Instance.PlaySFX(AudioManager.Instance.menuSelectSound);
        optionsScreen.SetActive(false);
        mainScreen.SetActive(true);
    }

    public void QuitGame()
    {
        AudioManager.Instance.PlaySFX(AudioManager.Instance.menuClickSound);
        GameManager.Instance.QuitGame();
    }

    public void OnMasterVolumeChanged(float value)
    {
        AudioManager.Instance.SetMasterVolume(value);
    }

    public void OnMusicVolumeChanged(float value)
    {
        AudioManager.Instance.SetMusicVolume(value);
    }

    public void OnSFXVolumeChanged(float value)
    {
        AudioManager.Instance.SetSFXVolume(value);
    }
}
