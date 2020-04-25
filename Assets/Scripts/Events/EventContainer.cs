using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EventContainer
{
    public static readonly TypedEvent<(Vector3, float)> UPDATE_FOG_OF_WAR = new TypedEvent<(Vector3, float)>();
}
