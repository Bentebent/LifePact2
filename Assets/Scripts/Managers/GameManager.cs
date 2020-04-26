using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private GameObject _playerContainer = null;

    [SerializeField]
    private Player _player = null;

    private int _currentLevel = 0;

    private void Awake()
	{
        GenerateMap();
	}
	
    private void Start()
    {
        
    }

    private void Update()
    {
    }

    private void GenerateMap()
    {
        MapManager.Instance.GenerateMap(DateTime.Now.Ticks, _currentLevel);
        MapManager.Instance.PopulateMap(_currentLevel);
        _playerContainer.transform.position = MapManager.Instance.Map.PlayerSpawnPosition;
        _playerContainer.SetActive(true);
    }
}
