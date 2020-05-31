using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class WinManager : MonoBehaviour
{
    public Image fadePanel;

    public Text scoreText;

    private bool focused = false;

    private void Awake()
    {
        focused = false;
        scoreText.text = "Score: " + FormatScore();
        StartCoroutine(FadeOutCover());
    }

    private string FormatScore()
    {
        string scoreStringA = OptionHolder.score.ToString();
        int scoreLengthA = scoreStringA.Length;

        if (scoreLengthA > 3)
        {
            int i = 0;
            string scoreStringB = scoreStringA;
            for (int c = 0; c < scoreLengthA; c++)
            {
                if (i >= 3)
                {
                    scoreStringB = scoreStringB.Insert(scoreLengthA - c, ",");
                    i = 0;
                }
                i++;
            }
            return scoreStringB;
        }
        return scoreStringA;
    }

    public void RestartButton()
    {
        StartCoroutine(FadeInCover(false));
    }

    public void TitleButton()
    {
        StartCoroutine(FadeInCover(true));
    }

    public void QuitButton()
    {
        Application.Quit();
    }

    private IEnumerator FadeOutCover()
    {
        fadePanel.gameObject.SetActive(true);
        fadePanel.color = new Color(255, 202, 24, 255);
        for (float c = 255; c > 0; c -= 4)
        {
            fadePanel.color = new Color(255, 202, 24, c / 255);
            yield return new WaitForEndOfFrame();
        }
        fadePanel.color = Color.clear;
        fadePanel.gameObject.SetActive(false);
        OptionHolder.transitioning = false;
    }

    private IEnumerator FadeInCover(bool title)
    {
        fadePanel.gameObject.SetActive(true);
        fadePanel.color = Color.clear;
        for (float c = 0; c < 255; c += 4)
        {
            fadePanel.color = new Color(0, 0, 0, c / 255);
            yield return new WaitForEndOfFrame();
        }
        fadePanel.color = Color.black;

        if (title)
            SceneManager.LoadSceneAsync(0);
        else
            SceneManager.LoadSceneAsync(1);
    }

    void Update()
    {
        if (!focused)
        {
            if (Input.GetButtonDown("Cancel"))
            {
                EventSystem.current.SetSelectedGameObject(GameObject.Find("Quit Button").gameObject);
                focused = true;
            }
            else if (Input.anyKeyDown)
            {
                EventSystem.current.SetSelectedGameObject(GameObject.Find("Title Button").gameObject);
                focused = true;
            }
        }
    }
}
