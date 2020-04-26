using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stairs : MonoBehaviour
{
	private void Awake()
	{
		
	}
	
    private void Start()
    {
        
    }

    private void Update()
    {
        
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == Layers.Player)
        {
            EventContainer.LEVEL_FINISHED.Dispatch();
        }
    }
}
