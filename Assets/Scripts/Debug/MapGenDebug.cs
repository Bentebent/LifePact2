using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapGenDebug : MonoBehaviour
{
    public long Seed;
    public int Level;

    public GameObject PlayerContainerPrefab = null;

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

        if (Input.GetKeyDown(KeyCode.P))
        {
            MapManager.Instance.PopulateMap(Level);
        }

        if (Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            MapManager.Instance.GenerateMap(++Seed, Level);
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            GameObject playerGO = Instantiate(PlayerContainerPrefab, MapManager.Instance.Map.PlayerSpawnPosition,
                Quaternion.identity);
        }

        EventContainer.UPDATE_FOG_OF_WAR.Dispatch((Camera.main.transform.position, 30.0f));
    }
}
