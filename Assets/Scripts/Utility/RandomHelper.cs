using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RandomHelper
{
    public static Vector2 RandomPointInCircle(float radius)
    {
        float r = radius * Mathf.Sqrt(UnityEngine.Random.Range(0.0f, 1.0f));
        float theta = UnityEngine.Random.Range(0.0f, 1.0f) * 2 * Mathf.PI;

        float x = r * Mathf.Cos(theta);
        float y = r * Mathf.Sin(theta);

        return new Vector2(x, y);
    }
}
