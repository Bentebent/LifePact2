using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapManager : MonoBehaviour
{
    [SerializeField]
    private DungeonData _dungeonData = null;

    [SerializeField]
    private Tilemap _floor = null;

    [SerializeField]
    private Tilemap _walls = null;

    [SerializeField]
    private Tilemap _pits = null;

    [SerializeField]
    private Tilemap _statistics = null;

    [SerializeField]
    private Tilemap _collision = null;

    [SerializeField]
    private Tilemap _pitCollision = null;

    [SerializeField]
    private NavigationManager _navigation = null;

    public Map Map => _map;
    public NavigationManager Navigation => _navigation;
    public bool DrawDebug { get; set; }

    private Map _map = null;
    private MapGenerator _mapGenerator = null;
    private static MapManager _instance = null;
    public static MapManager Instance
    {
        get
        {
            if (_instance != null)
            {
                return _instance;
            }

            _instance = FindObjectOfType<MapManager>();

            if (_instance == null || _instance.Equals(null))
            {
                Debug.LogError("The scene needs a MapManager");
            }


            return _instance;
        }
    }

    private void Awake()
    {
        _mapGenerator = new MapGenerator();
        _mapGenerator.Initialize(_navigation, _dungeonData, _floor, _walls, _pits, _statistics, _collision, 
            _pitCollision);
    }

    private void Start()
    {
        
    }

    private void OnDrawGizmos()
    {
        if (DrawDebug && _map != null)
        {
            _map.DrawDebug();
        }
    }

    private void Update()
    {
        
    }

    public void GenerateMap(long seed, int level)
    {
        if (_map != null)
        {
            _map.ClearMap();
        }

        _map = _mapGenerator.GenerateMap(seed, level);
    }

    public void PopulateMap(int level)
    {
        _mapGenerator.PopulateMap(ref _map, level);
    }
}
