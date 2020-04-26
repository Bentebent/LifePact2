using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EventContainer
{
    public static readonly TypedEvent<Player> SPAWN_PLAYER = new TypedEvent<Player>();

    public static void ClearEventListeners()
    {
        SPAWN_PLAYER.Clear();
    }
}