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
    [SerializeField]
    private bool _fogOfWarEnabled = true;

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

        EventContainer.UPDATE_FOG_OF_WAR.AddListener(UpdateFogOfWar);
    }

    private void Start()
    {
        
    }

    private void OnDrawGizmos()
    {
        if (DrawDebug)
        {
            _map?.DrawDebug();
        }
    }

    private void Update()
    {
        
    }

    public void UpdateFogOfWar((Vector3 position, float viewDistance) data)
    {
        if (_map != null && _fogOfWarEnabled)
        {
            _fogOfWar.UpdateFogOfWar(data.position, data.viewDistance, ref _fogOfWarRenderer);
        }
    }

    public void ToggleFogOfWar(bool visible)
    {
        if (visible && !_fogOfWarEnabled)
        {
            _fogOfWar.GenerateTexture(_floor, _walls, ref _fogOfWarRenderer);
        }

        _fogOfWarEnabled = visible;
        _fogOfWarRenderer.enabled = _fogOfWarEnabled;
    }

    public void GenerateMap(long seed, int level)
    {
        _map?.ClearMap();
        _map = _mapGenerator.GenerateMap(seed, level);

        if (_fogOfWarEnabled)
        {
            _fogOfWar.GenerateTexture(_floor, _walls, ref _fogOfWarRenderer);
        }
    }

    public void LoadSceneMap(List<GameObject> interactiveObjects = null, List<GameObject> enemies = null)
    {
        _map?.ClearMap();

        if (interactiveObjects == null)
        {
            interactiveObjects = new List<GameObject>();
            interactiveObjects.AddRange(GameObject.FindGameObjectsWithTag("InteractiveObject"));
        }

        if (enemies == null)
        {
            enemies = new List<GameObject>();
            enemies.AddRange(GameObject.FindGameObjectsWithTag("Enemy"));
        }

        _map = new Map(_floor, _walls, _pits, _statistics, _collision, _pitCollision, new MillerParkLCG(), _navigation,
            interactiveObjects, enemies);
       
        if (_fogOfWarEnabled)
        {
            _fogOfWar.GenerateTexture(_floor, _walls, ref _fogOfWarRenderer);
        }
    }

    public void PopulateMap(int level)
    {
        _mapGenerator.PopulateMap(ref _map, level);
    }
}
