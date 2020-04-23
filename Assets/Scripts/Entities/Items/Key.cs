using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Key : Item
{
    public Door Owner = null;
    public bool IsGoldenKey = false;

    public bool Consumed { get; set; }
    protected override void ApplyEffect(GameObject owner)
    {
        //Add key to player
    }

    private void Awake()
	{
		
	}
	
    private void Start()
    {
        
    }

    private void Update()
    {
        
    }
}
