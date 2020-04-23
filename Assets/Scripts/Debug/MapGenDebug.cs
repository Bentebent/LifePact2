using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapGenDebug : MonoBehaviour
{
    public long Seed;
    public int Level;

    private void Start()
    {
        MapManager.Instance.DrawDebug = true;
    }

    private void OnDrawGizmos()
    {

    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            MapManager.Instance.GenerateMap(Seed, Level);
        }

        if (Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            MapManager.Instance.GenerateMap(++Seed, Level);
        }
    }
}
