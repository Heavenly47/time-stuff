using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class MainManager : MonoBehaviour
{
    public Image fadePanel;

    private bool focused = false;

    private void Awake()
    {
        focused = false;
        StartCoroutine(FadeOutCover());
    }

    public void StartButton()
    {
        StartCoroutine(FadeInCover());
    }

    public void QuitButton()
    {
        Application.Quit();
    }

    private IEnumerator FadeOutCover()
    {
        fadePanel.gameObject.SetActive(true);
        fadePanel.color = Color.black;
        for (float c = 255; c > 0; c -= 4)
        {
            fadePanel.color = new Color(0, 0, 0, c / 255);
            yield return new WaitForEndOfFrame();
        }
        fadePanel.color = Color.clear;
        fadePanel.gameObject.SetActive(false);
    }

    private IEnumerator FadeInCover()
    {
        OptionHolder.transitioning = true;
        fadePanel.gameObject.SetActive(true);
        fadePanel.color = Color.clear;
        for (float c = 0; c < 255; c += 4)
        {
            fadePanel.color = new Color(0, 0, 0, c / 255);
            yield return new WaitForEndOfFrame();
        }
        fadePanel.color = Color.black;
        SceneManager.LoadSceneAsync(1);
    }

    void Update()
    {
        if (Input.GetButtonDown("Cancel"))
            Application.Quit();
        else if (!focused && Input.anyKeyDown)
        {
            EventSystem.current.SetSelectedGameObject(GameObject.Find("Play Button").gameObject);
            focused = true;
        }
    }
}
