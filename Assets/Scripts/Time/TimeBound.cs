//Copyright © 2020 Benjamin Robinson - Heavenly47

using UnityEngine;

public class TimeBound : MonoBehaviour
{
    public bool freezeAfter = false;
    public int sectionTime;

    public int cameraSize = 7;

    //Start with use pop text false if no text ever be displayed, changed to false after used
    public bool usePopText = false;
    public string popText;

    public bool useHintGate = false;
    public int hintGate = 1;
    public string hintText;
    private int sectionFails = 0;

    public GameObject[] boundedObjects;

    public void SetBound()
    {
        TimeManager.ClearList();
        foreach (GameObject newBound in boundedObjects)
        {
            newBound.GetComponent<PositionLogger>().SetActive(sectionTime);
        }
    }

    public void ResetBound()
    {
        foreach (GameObject oldBound in boundedObjects)
        {
            oldBound.GetComponent<PositionLogger>().ResetActive();
        }
    }

    public bool HintGateReady()
    {
        if (useHintGate)
        {
            sectionFails++;
            if (sectionFails >= hintGate)
            {
                useHintGate = false;
                return true;
            }
        }
        return false;
    }
}
