using UnityEngine;
using UnityEngine.EventSystems;

public class Pause : MonoBehaviour
{
    [Header("Escape Menu")]
    public GameObject pausePanel;                   // The panel that holds all of the escape menu buttons
    public GameObject menuA;                        //The gameobject of the parent that holds the MENU message ("Are you sure you want to return to menu") with the choice buttons as children
    public GameObject quitA;                        //The gameobject of the parent that holds the QUIT message ("Are you sure you want to quit") with the choice buttons as children
    public GameObject mainPanel;
    [Header("Other")]
    public GameObject EmptyFocus;                   //An empty transform object that is located off of the viewable canvas that serves to allow for easy position change for the entire menu

    //reveals pause content while play is paused
    private void PauseSet()
    {
        Time.timeScale = 0;
        pausePanel.SetActive(true);
        EventSystem.current.SetSelectedGameObject(mainPanel.transform.Find("Resume Button").gameObject);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        OptionHolder.paused = true;
    }

    //hides pause content to resume play
    public void PauseReset()
    {
        Time.timeScale = 1;
        mainPanel.SetActive(true);
        pausePanel.SetActive(false);
        EventSystem.current.SetSelectedGameObject(EmptyFocus.gameObject);
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;
        menuA.SetActive(false);
        quitA.SetActive(false);

        OptionHolder.paused = false;
    }

    void Update()
    {
        //Testing to see if player presses esc button (keyboard) or the menu button (controller)
        if (Input.GetButtonDown("Cancel"))
        {
            //Checks if the game is already paused or if it should pause
            if (!OptionHolder.paused)
                PauseSet();
            else
                PauseReset();
        }

        //Checks to see if B button is pressed on controler to exit pause
        if (OptionHolder.paused && Input.GetKeyDown(KeyCode.Joystick1Button1))
            PauseReset();
    }
}

