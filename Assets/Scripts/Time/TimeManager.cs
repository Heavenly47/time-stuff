using System.Collections.Generic;
using UnityEngine;

public static class TimeManager
{
    //This is a list of all current objects that should be tracked for the possiblity of time reversing
    private static List<PositionLogger> physicalObjects = new List<PositionLogger>();

    public static int timeRemaining = 0;
    public static bool timeManipulated = false;

    public static void AddToList (PositionLogger obj)
    {
        physicalObjects.Add(obj);
    }

    public static void RemoveFromList (PositionLogger obj)
    {
        physicalObjects.Remove(obj);
    }

    public static void ClearList()
    {
        foreach (PositionLogger obj in physicalObjects)
        {
            obj.RemoveActive();
        }
        physicalObjects.Clear();
    }

    public static void FreezeAllObjects()
    {
        if (timeRemaining > 0)
        {
            foreach (PositionLogger obj in physicalObjects)
            {
                obj.PreRewind();
            }
            timeManipulated = true;
        }
    }

    public static void ResumeAllObjects()
    {
        timeManipulated = false;
        foreach (PositionLogger obj in physicalObjects)
        {
            obj.PostRewind();
        }
    }

    public static void RewindTime()
    {
        if (timeRemaining > 0)
        {
            foreach (PositionLogger obj in physicalObjects)
            {
                obj.Rewind();
            }
        }
    }
}
