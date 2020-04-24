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
    private SpriteRenderer _fogOfWarRenderer = null;

    public Map Map => _map;
    public NavigationManager Navigation => _navigation;
    public bool DrawDebug { get; set; }

    private Map _map = null;
    private MapGenerator _mapGenerator = null;
    private FogOfWar _fogOfWar = null;
    private NavigationManager _navigation = null;

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
        _navigation = new NavigationManager();
        _mapGenerator = new MapGenerator();
        _mapGenerator.Initialize(_navigation, _dungeonData, _floor, _walls, _pits, _statistics, _collision, 
            _pitCollision);

        _fogOfWar = new FogOfWar();
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
        if (_map != null)
        {
            _fogOfWar.UpdateFogOfWar(Camera.main.transform.position, 30.0f, ref _fogOfWarRenderer);
        }
    }

    public void GenerateMap(long seed, int level)
    {
        if (_map != null)
        {
            _map.ClearMap();
        }

        _map = _mapGenerator.GenerateMap(seed, level);
        _fogOfWar.GenerateTexture(_floor, _walls, ref _fogOfWarRenderer);
    }

    public void PopulateMap(int level)
    {
        _mapGenerator.PopulateMap(ref _map, level);
    }
}
