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
        EventContainer.LEVEL_FINISHED.AddListener(LoadNextLevel);

        GenerateMap();
	}
	
    private void Start()
    {
        
    }

    private void Update()
    {
    }

    private void LoadNextLevel()
    {
        _playerContainer.SetActive(false);

        _currentLevel++;
        GenerateMap();
    }

    private void GenerateMap()
    {
        MapManager.Instance.GenerateMap(DateTime.Now.Ticks, _currentLevel);
        MapManager.Instance.PopulateMap(_currentLevel);
        _player.Teleport(MapManager.Instance.Map.PlayerSpawnPosition);
        _playerContainer.SetActive(true);
    }
}
