using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EventContainer
{
    public static readonly TypedEvent<(Vector3, float)> UPDATE_FOG_OF_WAR = new TypedEvent<(Vector3, float)>();
    public static readonly TypedEvent<Player> SPAWN_PLAYER = new TypedEvent<Player>();

    public static void ClearEventListeners()
    {
        UPDATE_FOG_OF_WAR.Clear();
        SPAWN_PLAYER.Clear();
    }
}