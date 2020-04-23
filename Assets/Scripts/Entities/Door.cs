using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    public RectInt Bounds { get; set; }
    public bool IsGoalDoor { get; set; }
    public List<Door> Siblings = new List<Door>();

    private bool _closed = true;
    private bool _locked = true;

    private void Awake()
	{
		
	}
	
    private void Start()
    {
        
    }

    private void Update()
    {
        
    }

    public void Open(bool unlock)
    {
        if (unlock)
        {
            _locked = false;
        }

        _closed = false;
    }

    public void Close(bool lockDoor)
    {
        if (lockDoor)
        {
            _locked = true;
        }

        _closed = true;
    }
}
