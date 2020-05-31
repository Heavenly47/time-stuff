using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class PauseController : MonoBehaviour
{
    //Escape menu
    [Header("Escape Menu")]
    public GameObject pausePanel;
    public GameObject mainPanel;
    public GameObject menuCheckPanel;
    public GameObject quitCheckPanel;
    public Image fadeImage;

    [Header("Audio")]
    public AudioSource masterAudio;

    private int fadeSpeed = 4;

    void Start()
    {
        StartCoroutine(FadeOutCover());
        StartCoroutine(FadeInAudio());
    }

    public void VolumeChanged()
    {
        OptionHolder.audioVolume = mainPanel.transform.Find("Volume Slider").GetComponent<Slider>().value;
        masterAudio.volume = OptionHolder.audioVolume;
    }

    public void MenuCheckShow()
    {
        menuCheckPanel.SetActive(true);
        EventSystem.current.SetSelectedGameObject(menuCheckPanel.transform.Find("No Button").gameObject);
    }

    public void MenuCheckHide()
    {
        menuCheckPanel.SetActive(false);
        EventSystem.current.SetSelectedGameObject(mainPanel.transform.Find("Main Button").gameObject);
    }

    public void MenuQuit()
    {
        Debug.Log("Quit to Menu");
        StartCoroutine(FadeOutAudio());
        StartCoroutine(FadeInCover());
        Time.timeScale = 1;
        OptionHolder.paused = false;
    }

    public void QuitCheckShow()
    {
        quitCheckPanel.SetActive(true);
        EventSystem.current.SetSelectedGameObject(quitCheckPanel.transform.Find("No Button").gameObject);
    }

    public void QuitCheckHide()
    {
        quitCheckPanel.SetActive(false);
        EventSystem.current.SetSelectedGameObject(mainPanel.transform.Find("Quit Button").gameObject);
    }

    public void Quit()
    {
        Debug.Log("Quit Game");
        Application.Quit();
    }

    private IEnumerator FadeOutCover()
    {
        fadeImage.gameObject.SetActive(true);
        fadeImage.color = Color.black;
        for (float c = 255; c > 0; c -= fadeSpeed)
        {
            fadeImage.color = new Color(0, 0, 0, c / 255);
            yield return new WaitForEndOfFrame();
        }
        fadeImage.color = Color.clear;
        fadeImage.gameObject.SetActive(false);
        OptionHolder.transitioning = false;
    }

    private IEnumerator FadeInCover()
    {
        OptionHolder.transitioning = true;
        fadeImage.gameObject.SetActive(true);
        fadeImage.color = Color.clear;
        for (float c = 0; c < 255; c += fadeSpeed)
        {
            fadeImage.color = new Color(0, 0, 0, c / 255);
            yield return new WaitForEndOfFrame();
        }
        fadeImage.color = Color.black;

        OptionHolder.transitioning = false;
        SceneManager.LoadSceneAsync(0);
    }

    private IEnumerator FadeInAudio()
    {
        masterAudio.volume = 0;
        for (float a = 0; a < OptionHolder.audioVolume * 255; a += fadeSpeed)
        {
            masterAudio.volume = a / 255;
            yield return new WaitForEndOfFrame();
        }
        masterAudio.volume = OptionHolder.audioVolume;
    }

    private IEnumerator FadeOutAudio()
    {
        masterAudio.volume = OptionHolder.audioVolume;
        for (float a = OptionHolder.audioVolume * 255; a > 0; a -= fadeSpeed)
        {
            masterAudio.volume = a / 255;
            yield return new WaitForEndOfFrame();
        }
        masterAudio.volume = 0;
    }

    public IEnumerator FadeToWin()
    {
        fadeImage.gameObject.SetActive(true);
        fadeImage.color = Color.clear;
        for (float c = 0; c < 255; c += fadeSpeed)
        {
            fadeImage.color = new Color(255, 202, 24, c / 255);
            yield return new WaitForEndOfFrame();
        }
        fadeImage.color = new Color(255, 202, 24, 255);

        SceneManager.LoadSceneAsync(2);
    }
}