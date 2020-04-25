using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Map
{
    public List<MapNode> Cells { get; set; }
    public List<Delaunay.Triangle<MapNode>> Triangles { get; set; }
    public List<Delaunay.Edge<MapNode>> DelaunayGraph { get; set; }
    public List<Delaunay.Edge<MapNode>> GabrielGraph { get; set; }
    public List<Delaunay.Edge<MapNode>> EMSTGraph { get; set; }
    public List<Delaunay.Edge<MapNode>> LayoutGraph { get; set; }
    public List<LineSegment2D> StartToGoalPath { get; set; }
    public List<BoundsInt> ChokePoints { get; set; }
    public int[,] CollisionMap { get; set; }
    public TilemapCollider2D CollisionMapCollider { get; set; }
    public BoundsInt Bounds { get; set; }

    public List<GameObject> InteractiveObjects { get; }
    public List<GameObject> Enemies { get; }

    private readonly bool _drawCells;
    private readonly bool _drawDelaunay;
    private readonly bool _drawGabriel;
    private readonly bool _drawEMST;
    private readonly bool _drawLayout;
    private readonly bool _drawBounds;

    private readonly Tilemap _floors = null;
    private readonly Tilemap _walls = null;
    private readonly Tilemap _pits = null;
    private readonly Tilemap _statistics = null;
    private readonly Tilemap _collision = null;
    private readonly Tilemap _pitCollision = null;

    private readonly LCG _random = null;
    private readonly NavigationManager _navigation = null;

    public Map(Tilemap floors, Tilemap walls, Tilemap pits, Tilemap statistics, Tilemap collision, Tilemap pitCollision, 
        LCG random, NavigationManager navigation)
    {
        _drawCells = true;
        _drawDelaunay = false;
        _drawGabriel = false;
        _drawEMST = false;
        _drawLayout = true;
        _drawBounds = false;

        _floors = floors;
        _walls = walls;
        _pits = pits;
        _statistics = statistics;
        _collision = collision;
        _pitCollision = pitCollision;

        _random = random;
        _navigation = navigation;

        InteractiveObjects = new List<GameObject>();
        Enemies = new List<GameObject>();
        ChokePoints = new List<BoundsInt>();
        StartToGoalPath = new List<LineSegment2D>();

        CollisionMapCollider = collision.gameObject.GetComponent<TilemapCollider2D>();
    }

    public Map(Tilemap floors, Tilemap walls, Tilemap pits, Tilemap statistics, Tilemap collision, Tilemap pitCollision,
       LCG random, NavigationManager navigation, List<GameObject> interactiveObjects, List<GameObject> enemies)
        : this(floors, walls, pits, statistics, collision, pitCollision, random, navigation)
    {
        InteractiveObjects = interactiveObjects;
        Enemies = enemies;

        Bounds = new BoundsInt(_walls.origin, _walls.size);
        CollisionMap = new int[Bounds.size.x, Bounds.size.y];

        for (int i = 0; i < CollisionMap.GetLength(0); i++)
        {
            for (int j = 0; j < CollisionMap.GetLength(1); j++)
            {
                CollisionMap[i, j] = 0;
                Vector3Int pos = new Vector3Int(Bounds.xMin + i, Bounds.yMin + j, 0);

                if (_walls.GetTile(pos) != null)
                {
                    CollisionMap[i, j] = 1;
                }
                else if(_pits.GetTile(pos) != null)
                {
                    CollisionMap[i, j] = 2;
                }
            }
        }

        UpdateCollisionTilemap(true);
        UpdateCollisionTilemap(false);
    }

    public void AddInteractiveObject(GameObject interactiveObject)
    {
        InteractiveObjects.Add(interactiveObject);
    }

    public void AddEnemy(GameObject enemy)
    {
        Enemies.Add(enemy);
    }

    public void ClearMap()
    {
        _floors.ClearAllTiles();
        _walls.ClearAllTiles();
        _pits.ClearAllTiles();

        ClearCollisionMaps();

        DestroyAllInteractiveObjects();
    }

    private void ClearCollisionMaps()
    {
        _collision.ClearAllTiles();
        _pitCollision.ClearAllTiles();

        CollisionMap = new int[CollisionMap.GetLength(0),CollisionMap.GetLength(1)];
    }

    public void ActivateObjects()
    {
        Enemies.ForEach(x => x.SetActive(true));
    }

    public List<Enemy> GetEnemiesInCircle(Vector2 position, float radius)
    {
        List<Enemy> closeEnemies = new List<Enemy>();
        foreach (GameObject enemy in Enemies)
        {
            if (enemy == null || enemy.Equals(null))
            {
                continue;
            }
    
            if (Vector2.Distance(enemy.transform.position, position) <= radius)
            {
                closeEnemies.Add(enemy.GetComponent<Enemy>());
            }
        }
        return closeEnemies;
    }

    public Vector2 GetPositionInMap(int widthInTiles, int heightInTiles, bool includeCorridorRooms, out MapNode room,
        List<MapNode> excludedRooms = null)
    {
        room = GetRandomRoom(widthInTiles, heightInTiles, includeCorridorRooms, excludedRooms);
        return GetRandomPositionInRoom(widthInTiles, heightInTiles, room);
    }

    public Vector2 GetPositionInMap(int widthInTiles, int heightInTiles, bool includeCorridorRooms, 
        List<MapNode> excludedRooms = null)
    {
        return GetRandomPositionInRoom(widthInTiles, heightInTiles, 
            GetRandomRoom(widthInTiles, heightInTiles, includeCorridorRooms, excludedRooms));
    }

    public Tuple<MapNode, MapNode> GetRoomsFurthestApart(bool lockableOnly, out List<MapNode> path)
    {
        List<MapNode> edgeNodes = new List<MapNode>();
        path = null;
        int edgeCount = 1;
        while(edgeNodes.Count < 2)
        {
            edgeNodes.AddRange(Cells.Where(x => x.Corridors.Count == edgeCount && (!lockableOnly || x.Lockable)));
            edgeCount++;

            //exit condition
            if (edgeCount > 100)
            {
                return null;
            }
        }

        if (edgeNodes.Count == 2)
        {
            path = _navigation.AStar(edgeNodes[0], edgeNodes[1], out float distance);
            return new Tuple<MapNode, MapNode>(edgeNodes[0], edgeNodes[1]);
        }

        Tuple<MapNode, MapNode> result = null;
        float maxDistance = float.MinValue;

        for(int i = 0; i < edgeNodes.Count; i++)
        {
            for (int j = 0; j < edgeNodes.Count; j++)
            {
                if (edgeNodes[i].Equals(edgeNodes[j]))
                {
                    continue;
                }

                List<MapNode> tempPath = _navigation.AStar(edgeNodes[i], edgeNodes[j], out float distance);
                if (distance > maxDistance)
                {
                    path = tempPath;
                    maxDistance = distance;
                    result = new Tuple<MapNode, MapNode>(edgeNodes[i], edgeNodes[j]);
                }
            }
        }

        return result;
    }

    public MapNode GetRandomRoom(int tileWidth, int tileHeight, bool includeCorridorRooms, List<MapNode> excludedRooms = null)
    {
        MapNode room = null;
        int cellCount = includeCorridorRooms ? Cells.Count() : Cells.Count(x => x.Type == MapNodeType.Room);

        while (room == null)
        {
            MapNode potential = Cells[_random.Range(0, Cells.Count)];

            if (!includeCorridorRooms && potential.Type == MapNodeType.Corridor)
            {
                continue;
            }

            if (excludedRooms != null && excludedRooms.Contains(potential) && excludedRooms.Count < cellCount)
            {
                continue;
            }

            int floorWidth = potential.Cell.width - 2;
            int floorHeight = potential.Cell.height - 2;

            if (floorWidth < tileWidth || floorHeight < tileHeight)
            {
                continue;
            }

            room = potential;
        }

        return room;
    }

    public Vector2 GetRandomPositionInRoom(int widthInTiles, int heightInTiles, MapNode room, int attempts = 10)
    {
        int halfWidth = Mathf.Max(Mathf.RoundToInt(widthInTiles / 2.0f), 1);
        int halfHeight = Mathf.Max(Mathf.RoundToInt(heightInTiles / 2.0f), 1);
        
        int startingPosX = _random.Range(room.Cell.xMin + halfWidth, room.Cell.xMax - halfWidth);
        int startingPosY = _random.Range(room.Cell.yMin + halfHeight, room.Cell.yMax - halfHeight);

        int spawnX = startingPosX;
        int spawnY = startingPosY;
        while (true)
        {
            BoundsInt bounds = new BoundsInt(spawnX - halfWidth, spawnY - halfHeight, 0, halfWidth * 2, halfHeight * 2, 0);
        
            if (!HasCollisionIndex(bounds))
            {
                return new Vector2(spawnX, spawnY);
            }
            else
            {
                spawnY += heightInTiles;

                if (spawnY >= room.Cell.yMax - halfHeight)
                {
                    spawnY = room.Cell.yMin + halfHeight;
                    spawnX += widthInTiles;
                }

                if (spawnX >= room.Cell.xMax - halfWidth)
                {
                    spawnX = room.Cell.xMin + halfWidth;
                }

                float dist = (new Vector2(spawnX, spawnY) - new Vector2(startingPosX, startingPosY)).sqrMagnitude;
                float range = widthInTiles - 0.1f;
                if (dist <= range * range)
                {
                    return Vector2.zero;
                }
            }
        }
    }

    public void DrawDebug()
    {
        if (_drawBounds && Bounds != null)
        {
            GizmoUtility.DrawRectangle(Bounds.ToRectInt(), Color.cyan);
        }

        if (_drawCells && Cells != null)
        {
            Cells.ForEach(x =>
            {
                switch (x.Type)
                {
                    case MapNodeType.Default:
                        {
                            GizmoUtility.DrawRectangle(x.Cell, Color.black);
                        }
                        break;
                    case MapNodeType.Room:
                        {
                            if (x.Lockable)
                            {
                                GizmoUtility.DrawRectangle(x.Cell, Color.green);
                            }
                            else
                            {
                                GizmoUtility.DrawRectangle(x.Cell, Color.red);
                            }
                        }
                        break;
                    case MapNodeType.Corridor:
                        {
                            GizmoUtility.DrawRectangle(x.Cell, Color.blue);
                        }
                        break;
                    default:
                        break;
                }
            });

            ChokePoints.ForEach(x =>
            {
                GizmoUtility.DrawRectangle(x.ToRectInt(), Color.magenta);
            });
        }

        if (_drawDelaunay && DelaunayGraph != null)
        {
            DelaunayGraph.ForEach(x =>
            {
                GizmoUtility.DrawLine(x, Color.cyan);
            });
        }

        if (_drawGabriel && GabrielGraph != null)
        {
            GabrielGraph.ForEach(x =>
            {
                GizmoUtility.DrawLine(x, Color.magenta);
            });
        }

        if (_drawEMST && EMSTGraph != null)
        {
            EMSTGraph.ForEach(x =>
            {
                GizmoUtility.DrawLine(x, Color.cyan);
            });
        }

        if (_drawLayout && LayoutGraph != null)
        {
            LayoutGraph.ForEach(x =>
            {
                GizmoUtility.DrawLine(x, Color.magenta);
            });

            if (StartToGoalPath != null)
            {
                StartToGoalPath.ForEach(x =>
                {
                    GizmoUtility.DrawLine(x, new Color(159 / 255.0f, 90 / 255.0f, 253 / 255.0f, 1));
                });
            }
        }
    }

    public void DrawPath(List<Vector2Int> path)
    {
        int index = 0; 
        path.ForEach(x =>
        {
            _floors.SetTileFlags(x.ToVector3Int(), TileFlags.None);

            Color tileColor = Color.blue;
            if (index == 0)
            {
                tileColor = Color.red;
            }
            else if (index == path.Count - 1)
            {
                tileColor = Color.yellow;
            }

            _floors.SetColor(x.ToVector3Int(), tileColor);
            index++;
        });
    }

    public void UpdateCollisionTilemap(bool walls)
    {
        Tilemap tilemap = walls ? _collision : _pitCollision;
        tilemap.ClearAllTiles();

        for(int i = 0; i < CollisionMap.GetLength(0); i++)
        {
            for(int j = 0; j < CollisionMap.GetLength(1); j++)
            {
                int collisionIndex = CollisionMap[i, j];
                Vector3Int pos = new Vector3Int(Bounds.xMin + i, Bounds.yMin + j, 0);

                if (walls && collisionIndex == 1)
                {
                    tilemap.SetTile(pos, _walls.GetTile(pos));
                }
                else if (!walls && collisionIndex == 2)
                {
                    tilemap.SetTile(pos, _pits.GetTile(pos));
                }
            }
        }
    }

    public void UpdateStatisticsMapDebug()
    {
        //Tilemap debug = GameObject.Find("Statistics").GetComponent<Tilemap>();
        //debug.ClearAllTiles();
        //
        //Cells.ForEach(room =>
        //{
        //    for(int x = room.Cell.xMin; x < room.Cell.xMax; x++)
        //    {
        //        for (int y = room.Cell.yMin; y < room.Cell.yMax; y++)
        //        {
        //            //debug.SetTile(new Vector3Int(x, y, 0), MapManager.Instance.selectedDungeonData.tileSet.FloorTiles.ElementAt(0).value);
        //            //debug.SetTileFlags(new Vector3Int(x, y, 0), TileFlags.None);
        //            //debug.SetColor(new Vector3Int(x, y, 0), Utility.RGBAColor(207, 0, 15, 
        //            //    Utility.ConvertRange(0.0f, 1.0f, 0.0f, 0.75f, room.SeclusionFactor)));
        //        }
        //    }
        //});
    }

    public void UpdateCollisionMap(RectInt bounds, int value, bool walls = true)
    {
        for(int i = 0; i < bounds.size.x; i++)
        {
            for (int j = 0; j < bounds.size.y; j++)
            {
                CollisionMap[bounds.xMin - Bounds.xMin + i, bounds.yMin - Bounds.yMin + j] = value;
            }
        }

        UpdateCollisionTilemap(walls);
    }

    public bool HasCollisionIndex(BoundsInt bounds)
    {
        for (int i = 0; i < bounds.size.x; i++)
        {
            for (int j = 0; j < bounds.size.y; j++)
            {
                int x = bounds.xMin - Bounds.xMin + i;
                int y = bounds.yMin - Bounds.yMin + j;

                if (x >= 0 && y >= 0 && x < CollisionMap.GetLength(0) && y < CollisionMap.GetLength(1))
                {
                    if (CollisionMap[x, y] > 0)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    public List<int> GetCollisionIndices(BoundsInt bounds)
    {
        List<int> result = new List<int>();
        for (int i = 0; i < bounds.size.x; i++)
        {
            for (int j = 0; j < bounds.size.y; j++)
            {
                result.Add(CollisionMap[bounds.xMin - Bounds.xMin + i, bounds.yMin - Bounds.yMin + j]);
            }
        }

        return result;
    }

    public int GetCollisionIndex(int x, int y)
    {
        int i = x - Bounds.xMin;
        int j = y - Bounds.yMin;

        if (i < 0 || i >= CollisionMap.GetLength(0) || j < 0 || j >= CollisionMap.GetLength(1))
        {
            return -1;
        }

        return CollisionMap[i, j];
    }

    public Vector2Int WorldToCell(Vector2 worldPos)
    {
        Vector3Int wtc = _floors.WorldToCell(worldPos.ToVector3());
        return new Vector2Int(wtc.x, wtc.y);
    }

    public Vector2 CellToWorld(Vector2Int cellPos)
    {
        Vector3 ctW = _floors.CellToWorld(cellPos.ToVector3Int());
        return ctW.ToVector2();
    }

    private void DestroyAllInteractiveObjects()
    {
        InteractiveObjects.ForEach(x =>
        {
            GameObject.Destroy(x);
        });
        InteractiveObjects.Clear();

        Enemies.ForEach(x =>
        {
            GameObject.Destroy(x);
        });
        Enemies.Clear();
    }
}
