using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum KeyType
{
    GOLD,
    SKELETON
}

public class Key : Item
{
    [SerializeField]
    private KeyType _type = KeyType.SKELETON;

    protected override void ApplyEffect(Player owner)
    {
        owner.AddKey(_type);
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
