using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EventContainer
{
    public static readonly UntypedEvent LEVEL_FINISHED = new UntypedEvent();

    public static void ClearEventListeners()
    {
        LEVEL_FINISHED.Clear();
    }
}