using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityDebug : MonoBehaviour
{
	private void Awake()
	{
		
	}
	
    private void Start()
    {
        
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Alpha1))
        {
            MapManager.Instance.LoadSceneMap();
        }
    }
}
