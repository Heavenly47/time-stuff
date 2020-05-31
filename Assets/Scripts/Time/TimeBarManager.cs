using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TimeBarManager : MonoBehaviour
{
    public RectTransform timeTransform;
    private float cachedY;
    private float maxXValue;
    private float minXValue;
    private int currentTime;
    public int CurrentTime
    {
        get { return currentTime; }
        set
        {
            currentTime = value;
            TimeManager.timeRemaining = value;
            if (maxTime < currentTime)
                maxTime = currentTime;
            HandleTime();
        }
    }
    public int maxTime;
    public Image visualTime;
    private int bufferedTime = 0;
    private bool useBuffer = false;

    void Start()
    {
        cachedY = timeTransform.position.y;
        maxXValue = timeTransform.position.x;
        minXValue = timeTransform.position.x - (timeTransform.rect.width * timeTransform.lossyScale.x);
        currentTime = maxTime;
    }

    void HandleTime()
    {
        float currentXValue = MapValues(currentTime, 0, maxTime, minXValue, maxXValue);
        timeTransform.position = new Vector3(currentXValue, cachedY);
    }

    private float MapValues(float x, float inMin, float inMax, float outMin, float outMax)
    {
        return (x - inMin) * (outMax - outMin) / (inMax - inMin) + outMin;
    }

    public IEnumerator IncreaseTime(int newTime)
    {
        int timeIncreaseValue = 10;

        bufferedTime = 0;
        useBuffer = true;
        maxTime = newTime;

        for (; currentTime < newTime - bufferedTime; CurrentTime += timeIncreaseValue)
        {
            if (newTime - currentTime - bufferedTime < 11)
                timeIncreaseValue = 1;
            yield return new WaitForEndOfFrame();
        }

        CurrentTime = newTime - bufferedTime;
        useBuffer = false;
        OptionHolder.timeStuffUsed += bufferedTime;
    }

    public void DecreaseTime(int time)
    {
        if (useBuffer)
            bufferedTime += time;
        else
        {
            if (currentTime - time > 0)
            {
                CurrentTime -= time;
                OptionHolder.timeStuffUsed += time;
            }
            else
                CurrentTime = 0;
        }
    }
}
