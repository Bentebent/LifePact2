using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class MapGenerator
{
    private DungeonData _selectedDungeonData = null;

    private Tilemap _floors = null;
    private Tilemap _walls = null;
    private Tilemap _pits = null;
    private Tilemap _statistics = null;
    private Tilemap _collision = null;
    private Tilemap _pitCollision = null;

    private MillerParkLCG _random = null;
    private NavigationManager _navigation = null;
    private Timer _timer = null;

    private MapPainter _mapPainter = null;
    private MapPopulator _mapPopulator = null;

    public void Initialize(NavigationManager navigation, DungeonData dungeonData, Tilemap floors, Tilemap walls, 
        Tilemap pits, Tilemap statistics, Tilemap collision, Tilemap pitCollision)
    {
        _navigation = navigation;
        _selectedDungeonData = dungeonData;
        _floors = floors;
        _walls = walls;
        _pits = pits;
        _statistics = statistics;
        _collision = collision;
        _pitCollision = pitCollision;

        _random = new MillerParkLCG();
        _timer = new Timer();
        _mapPainter = new MapPainter();
        _mapPainter.Initialize(_selectedDungeonData.TileSet, _selectedDungeonData.PitSet, _floors, _walls, _pits);

        _mapPopulator = new MapPopulator();
        _mapPopulator.Initialize(_random, navigation, _mapPainter, _selectedDungeonData.InteractiveObjects/*_selectedDungeonData.interactiveObjects, _selectedDungeonData.spawnables, 
            _selectedDungeonData.trapSet,*/);
    }

    public void GetSeed()
    {
        Debug.Log("Seed " + GetCurrentSeed() + " copied to clipboard");
        GUIUtility.systemCopyBuffer = GetCurrentSeed().ToString();
    }

    public long GetCurrentSeed()
    {
        return _random.GetCurrentSeed();
    }

    public Map GenerateMap(long seed, int level)
    {
        Debug.Log("Generating new map");
        Debug.Log(seed);

        _timer.Start();
        Map result = new Map(_floors, _walls, _pits, _statistics, _collision, _pitCollision, _random, _navigation);

        _random.SetSeed(seed);
                                                                                                                                                                                                             
        GenerateCells(ref result, _selectedDungeonData.Parameters);
        SeparateCells(ref result, _selectedDungeonData.Parameters);

        if (IdentifyRooms(ref result, _selectedDungeonData.Parameters) < 3)
        {
            seed += 1;
            return GenerateMap(seed, level);
        }

        Triangulate(ref result, _selectedDungeonData.Parameters);
        GenerateLayoutGraph(ref result, _selectedDungeonData.Parameters);
        GenerateCorridors(ref result, _selectedDungeonData.Parameters);
        PaintRooms(result, _selectedDungeonData.Parameters);
        _mapPainter.PaintTiles(result, _selectedDungeonData.Parameters);
        RemoveDeadRooms(ref result, _selectedDungeonData.Parameters);

        if(FindChokepoints(ref result, _selectedDungeonData.Parameters) < 2)
        {
            seed += 1;
            return GenerateMap(seed, level);
        }

        GeneratePools(ref result, _selectedDungeonData.Parameters);
        _mapPainter.PostProcessTiles(in result, in _selectedDungeonData.Parameters);

        result.UpdateCollisionTilemap(true);
        result.UpdateCollisionTilemap(false);

        _timer.Stop();
        _timer.Print("MapGenerator.GenerateMap");

        return result;
    }

    public void PopulateMap(ref Map map, int level)
    {
        _mapPopulator.PopulateMap(ref map,  _selectedDungeonData.Parameters, level);
    }

    private void GenerateCells(ref Map map, in MapGeneratorParameters parameters)
    {
        int cellCount = _random.Range(parameters.MinCellCount, parameters.MaxCellCount);
        map.Cells = new List<MapNode>(cellCount);

        int xMin = int.MaxValue;
        int xMax = int.MinValue;
        int yMin = int.MaxValue;
        int yMax = int.MinValue;

        for (int i = 0; i < cellCount; i++)
        {
            Vector2Int size = GenerateRandomSize(parameters);
            Vector2Int position = RandomPointInCircle(parameters.GenerationRadius);

            MapNode room = new MapNode(i, position, size);
            map.Cells.Add(new MapNode(i, position, size));
            map.Cells = map.Cells.OrderBy(x => x.Cell.x).ThenBy(x => x.Cell.y).ToList();

            if (room.Cell.xMin < xMin)
            {
                xMin = room.Cell.xMin;
            }
            if (room.Cell.xMax > xMax)
            {
                xMax = room.Cell.xMax;
            }
            if (room.Cell.yMin < yMin)
            {
                yMin = room.Cell.yMin;
            }
            if (room.Cell.yMax > yMax)
            {
                yMax = room.Cell.yMax;
            }
        }
        map.Bounds = new BoundsInt(xMin, yMin, 0, Mathf.Abs(xMax - xMin), Mathf.Abs(yMax - yMin), 0);
    }

    private void SeparateCells(ref Map map, in MapGeneratorParameters parameters)
    {
        bool regionsSeparated = false;
        int iterations = 0;

        int xMin = map.Bounds.xMin;
        int xMax = map.Bounds.xMax;
        int yMin = map.Bounds.yMin;
        int yMax = map.Bounds.yMax;

        while (!regionsSeparated && iterations < 2 * map.Cells.Count)
        {
            regionsSeparated = true;
            for (int i = 0; i < map.Cells.Count; i++)
            {
                Vector2 movement = Vector2.zero;
                int separationCount = 0;
                for (int j = 0; j < map.Cells.Count; j++)
                {
                    if (i == j)
                    {
                        continue;
                    }

                    RectInt overSizeNode = map.Cells[i].Cell;

                    if (parameters.MinRoomDistance > 0)
                    {
                        overSizeNode.position -= new Vector2Int(parameters.MinRoomDistance, parameters.MinRoomDistance);
                        overSizeNode.width += parameters.MinRoomDistance * 2;
                        overSizeNode.height += parameters.MinRoomDistance * 2;
                    }

                    if (!overSizeNode.Overlaps(map.Cells[j].Cell))
                    {
                        continue;
                    }

                    movement += map.Cells[j].Cell.center - overSizeNode.center;
                    separationCount++;
                }

                if (separationCount > 0)
                {
                    movement *= -1.0f;
                    movement = movement.normalized;
                    RectInt newRect = map.Cells[i].Cell;
                    newRect.position += new Vector2Int(Mathf.RoundToInt(movement.x), Mathf.RoundToInt(movement.y)) * (1 + parameters.MinRoomDistance);

                    if (newRect.position != map.Cells[i].Cell.position || movement.magnitude > 0.0f)
                    {
                        map.Cells[i].Cell = newRect;
                        regionsSeparated = false;
                    }

                    if (newRect.xMin < xMin)
                    {
                        xMin = newRect.xMin;
                    }
                    if (newRect.xMax > xMax)
                    {
                        xMax = newRect.xMax;
                    }
                    if (newRect.yMin < yMin)
                    {
                        yMin = newRect.yMin;
                    }
                    if (newRect.yMax > yMax)
                    {
                        yMax = newRect.yMax;
                    }
                }
            }
            iterations++;
        }

        map.Bounds = new BoundsInt(xMin - 1, yMin - 1, 0, Mathf.Abs(xMax - xMin) + 1, Mathf.Abs(yMax - yMin) + 1, 0);
        map.CollisionMap = new int[map.Bounds.size.x, map.Bounds.size.y];
        
       
        for(int i = 0; i < map.Cells.Count; i++)
        {
            if (map.Cells[i].Type == MapNodeType.None)
            {
                continue;
            }

            for(int j = i + 1; j < map.Cells.Count; j++)
            {
                if (i == j)
                {
                    continue;
                }

                if (map.Cells[j].Type == MapNodeType.None)
                {
                    continue;
                }

                if (map.Cells[i].Cell.Overlaps(map.Cells[j].Cell))
                {
                    map.Cells[j].Type = MapNodeType.None;
                }
            }
        }
    }

    private int IdentifyRooms(ref Map map, in MapGeneratorParameters parameters)
    {
        double roomThresholdX = 0;
        double roomThresholdY = 0;

        foreach (MapNode node in map.Cells)
        {
            roomThresholdX += node.Cell.width;
            roomThresholdY += node.Cell.height;
        }

        roomThresholdX /= map.Cells.Count;
        roomThresholdY /= map.Cells.Count;

        roomThresholdX *= parameters.RoomThresholdMultiplier;
        roomThresholdY *= parameters.RoomThresholdMultiplier;

        int roomCount = 0;
        foreach (MapNode node in map.Cells)
        {
            //These are already discarded
            if (node.Type == MapNodeType.None)
            {
                continue;
            }

            if(node.Cell.width > roomThresholdX && node.Cell.height > roomThresholdY)
            {
                node.Type = MapNodeType.Room;
                roomCount++;
            }
        }

        return roomCount;
    }

    private void Triangulate(ref Map map, in MapGeneratorParameters parameters)
    {
        Delaunay.BowerWatsonDelaunay triangulator = new Delaunay.BowerWatsonDelaunay();
        IEnumerable<Delaunay.Vertex<MapNode>> vertices = map.Cells.Where(x => x.Type == MapNodeType.Room)
            .Select(x => new Delaunay.Vertex<MapNode>(x.Cell.center, x)).Distinct();
        IEnumerable<Delaunay.Triangle<MapNode>> triangles = triangulator.Triangulate(vertices);
        map.Triangles = triangles.ToList();

        HashSet<Delaunay.Edge<MapNode>> delaunayEdges = triangulator.GetDelaunayEdges(triangles);
        HashSet<Delaunay.Edge<MapNode>> gabrielGraph = triangulator.GetGabrielGraph(delaunayEdges, vertices);
        HashSet<Delaunay.Edge<MapNode>> emst = triangulator.GetPrimEMST(gabrielGraph, vertices.Distinct());

        map.DelaunayGraph = delaunayEdges.ToList();
        map.GabrielGraph = gabrielGraph.ToList();
        map.EMSTGraph = emst.ToList();
    }

    private void GenerateLayoutGraph(ref Map map, in MapGeneratorParameters parameters)
    {
        List<Delaunay.Edge<MapNode>> result = new List<Delaunay.Edge<MapNode>>(map.EMSTGraph);
        List<Delaunay.Edge<MapNode>> allowedEdges = new List<Delaunay.Edge<MapNode>>();

        foreach (Delaunay.Edge<MapNode> edge in map.GabrielGraph)
        {
            if (!allowedEdges.Contains(edge) && !map.EMSTGraph.Contains(edge))
            {
                allowedEdges.Add(edge);
            }
        }

        int extraEdgeCount = (int)Math.Round(allowedEdges.Count * parameters.MazeFactor);
        for (int i = 0; i < extraEdgeCount; i++)
        {
            int edgeIndex = _random.Range(0, allowedEdges.Count);
            result.Add(allowedEdges[edgeIndex]);
            allowedEdges.RemoveAt(edgeIndex);
        }

        map.LayoutGraph = result;

        AddCorridorsToNodes(map);
    }

    private void AddCorridorsToNodes(Map map)
    {
        foreach (MapNode mapNode in map.Cells)
        {
            if (mapNode.Type == MapNodeType.Room)
            {
                mapNode.Corridors.Clear();
                List<Delaunay.Edge<MapNode>> edges = map.LayoutGraph.Where(y => y.ContainsVertex(new Delaunay.Vertex<MapNode>(mapNode.Cell.center, mapNode))).ToList();
                mapNode.Corridors.AddRange(edges);
            }
        }
    }

    private void PaintRooms(in Map map, in MapGeneratorParameters parameters)
    {
        foreach (MapNode node in map.Cells)
        {
            if (node.Type != MapNodeType.Room)
            {
                continue;
            }

            _mapPainter.PaintRoom(node.Cell, false);
        }
    }
 
    private void GenerateCorridors(ref Map map, in MapGeneratorParameters parameters)
    {
        List<Delaunay.Edge<MapNode>> result = new List<Delaunay.Edge<MapNode>>();
        foreach (Delaunay.Edge<MapNode> edge in map.LayoutGraph)
        {
            MapNode a = null;
            MapNode b = null;

            foreach (MapNode node in map.Cells)
            {
                if (node.Type != MapNodeType.Room)
                {
                    continue;
                }

                if (a == null && node.Cell.Contains(edge.Point1.Position))
                {
                    a = node;
                }
                else if (b == null && node.Cell.Contains(edge.Point2.Position))
                {
                    b = node;
                }

                if (a != null && b != null)
                {
                    break;
                }
            }

            if (a == null || b == null)
            {
                continue;
            }

            Vector2 aCenter = a.Cell.center;
            Vector2 bCenter = b.Cell.center;
            Vector2 midpoint = (aCenter + bCenter) / 2;

            bool useX = false;
            if (midpoint.x >= a.Cell.xMin 
                && midpoint.x <= a.Cell.xMax
                && midpoint.x >= b.Cell.xMin
                && midpoint.x <= b.Cell.xMax)
            {
                useX = true;
            }

            bool useY = false;
            if (midpoint.y >= a.Cell.yMin
                && midpoint.y <= a.Cell.yMax
                && midpoint.y >= b.Cell.yMin
                && midpoint.y <= b.Cell.yMax)
            {
                useY = true;
            }

            if (useX && useY)
            {
                if (_random.NextFloat() < 0.5f)
                {
                    useX = false;
                }
                else
                {
                    useY = false;
                }
            }

            if (useX)
            {
                int width = _random.Range(parameters.MinCorridorWidth, parameters.MaxCorridorWidth);
                int halfWidth = width / 2;
                
                int minX = (int)Mathf.Floor(midpoint.x) - halfWidth;
                int maxX = (int)Mathf.Floor(midpoint.x) + (width - halfWidth);
                int minY = (int)Mathf.Floor(Mathf.Min(aCenter.y, bCenter.y));
                int maxY = (int)Mathf.Floor(Mathf.Max(aCenter.y, bCenter.y));

                PaintCorridor(ref map, in parameters, minX, minY, Math.Abs(maxX - minX), Math.Abs(maxY - minY));
            }
            else if (useY)
            {
                int width = _random.Range(parameters.MinCorridorWidth, parameters.MaxCorridorWidth);
                int halfWidth = width / 2;
                
                int minX = (int)Mathf.Floor(Mathf.Min(aCenter.x, bCenter.x));
                int maxX = (int)Mathf.Floor(Mathf.Max(aCenter.x, bCenter.x));
                int minY = (int)Mathf.Floor(midpoint.y - halfWidth);
                int maxY = (int)Mathf.Floor(midpoint.y + (width - halfWidth));

                PaintCorridor(ref map, in parameters, minX, minY, Math.Abs(maxX - minX), Math.Abs(maxY - minY));
            }
            else
            {
                int width = _random.Range(parameters.MinCorridorWidth, parameters.MaxCorridorWidth);
                int halfWidth = width / 2;

                int minX = (int)Mathf.Floor(aCenter.x) - halfWidth;
                int maxX = (int)Mathf.Floor(aCenter.x);
                int minY = (int)Mathf.Floor(Mathf.Min(aCenter.y, bCenter.y)) - halfWidth;
                int maxY = (int)Mathf.Floor(Mathf.Max(aCenter.y, bCenter.y)) + width - halfWidth;

                PaintCorridor(ref map, in parameters, minX, minY, width, Math.Abs(maxY - minY));

                minX = (int)Mathf.Floor(Mathf.Min(aCenter.x, bCenter.x) - halfWidth);
                maxX = (int)Mathf.Floor(Mathf.Max(aCenter.x, bCenter.x));
                minY = (int)Mathf.Floor(bCenter.y) - halfWidth;
                maxY = (int)Mathf.Floor(bCenter.y);

                PaintCorridor(ref map, in parameters, minX, minY, Math.Abs(maxX - minX), width);
            }
        }
    }

    private void AddCorridorRooms(ref Map map, in MapGeneratorParameters parameters, BoundsInt corridorBounds)
    {
        foreach (MapNode room in map.Cells)
        {
            if (room.Type != MapNodeType.Default)
            {
                continue;
            }

            if (corridorBounds.Intersects(room.Cell, out RectInt area))
            {
                if (area.width > 2 && area.height > 2)
                {
                    room.Type = MapNodeType.Corridor;
                    _mapPainter.PaintRoom(room.Cell, false);
                }
            }
        }
    }

    private void PaintCorridor(ref Map map, in MapGeneratorParameters parameters, int minX, int minY, int width, int height)
    {
        Vector3Int pos = new Vector3Int(minX, minY, 0);
        Vector3Int size = new Vector3Int(width, height, 1);

        TileBase[] tiles = new TileBase[size.x * size.y];
        TileBase[] wallTiles = new TileBase[size.x * size.y];
        for (int tileIndex = 0; tileIndex < size.x * size.y; tileIndex++)
        {
            tiles[tileIndex] = _selectedDungeonData.TileSet.FloorTiles.GetRandom();
            wallTiles[tileIndex] = null;
        }

        BoundsInt bounds = new BoundsInt(pos, size);
        AddCorridorRooms(ref map, parameters, bounds);
        _floors.SetTilesBlock(bounds, tiles);
        _walls.SetTilesBlock(bounds, wallTiles);
    }
   
    private void RemoveDeadRooms(ref Map map, in MapGeneratorParameters parameters)
    {
        map.Cells.RemoveAll(x => x.Type == MapNodeType.Default || x.Type == MapNodeType.None);
    }

    private int FindChokepoints(ref Map map, in MapGeneratorParameters parameters)
    {
        int lockableRoomCount = 0;
        foreach (MapNode room in map.Cells)
        {
            bool roomLockable = true;
            RectInt overSizeRoom = room.Cell;
            int offset = 1;
            for (int i = 0; i < offset; i++)
            {
                BoundsInt upper = new BoundsInt();
                upper.xMin = overSizeRoom.xMin;
                upper.xMax = overSizeRoom.xMax;
                upper.yMin = overSizeRoom.yMax - 1 - i;
                upper.yMax = overSizeRoom.yMax - i;
                upper.zMax = 1;

                List<BoundsInt> result = FindChokepoints(upper, true, out bool fullWall, out bool lockable);
                room.Chokepoints.AddRange(result);
                map.ChokePoints.AddRange(result);

                if (!lockable)
                {
                    roomLockable = false;
                }

                if (result.Count > 0 || fullWall)
                {
                    break;
                }
            }
            
            for (int i = 0; i < offset; i++)
            {
                BoundsInt lower = new BoundsInt();
                lower.xMin = overSizeRoom.xMin;
                lower.xMax = overSizeRoom.xMax;
                lower.yMin = overSizeRoom.yMin + i;
                lower.yMax = overSizeRoom.yMin + 1 + i;
                lower.zMax = 1;
            
                List<BoundsInt> result = FindChokepoints(lower, true, out bool fullWall, out bool lockable);
                room.Chokepoints.AddRange(result);
                map.ChokePoints.AddRange(result);

                if (!lockable)
                {
                    roomLockable = false;
                }

                if (result.Count > 0 || fullWall)
                {
                    break;
                }
            }
            
            for (int i = 0; i < offset; i++)
            {
                BoundsInt left = new BoundsInt();
                left.xMin = overSizeRoom.xMin + i;
                left.xMax = overSizeRoom.xMin + 1 + i;
                left.yMin = overSizeRoom.yMin;
                left.yMax = overSizeRoom.yMax;
                left.zMax = 1;
            
                List<BoundsInt> result = FindChokepoints(left, false, out bool fullWall, out bool lockable);
                room.Chokepoints.AddRange(result);
                map.ChokePoints.AddRange(result);

                if (!lockable)
                {
                    roomLockable = false;
                }

                if (result.Count > 0 || fullWall)
                {
                    break;
                }
            }
            
            for (int i = 0; i < offset; i++)
            {
                BoundsInt right = new BoundsInt();
                right.xMin = overSizeRoom.xMax - 1 - i;
                right.xMax = overSizeRoom.xMax - i;
                right.yMin = overSizeRoom.yMin;
                right.yMax = overSizeRoom.yMax;
                right.zMax = 1;
            
                List<BoundsInt> result = FindChokepoints(right, false, out bool fullWall, out bool lockable);
                room.Chokepoints.AddRange(result);
                map.ChokePoints.AddRange(result);

                if (!lockable)
                {
                    roomLockable = false;
                }

                if (result.Count > 0 || fullWall)
                {
                    break;
                }
            }

            room.Lockable = roomLockable;
            if (room.Lockable)
            {
                lockableRoomCount++;
            }
        }

        return lockableRoomCount;
    }

    private List<BoundsInt> FindChokepoints(BoundsInt bounds, bool horizontal, out bool fullWall, out bool lockable)
    {
        fullWall = true;
        lockable = false;
        List<BoundsInt> result = new List<BoundsInt>();
        TileBase[] tempWalls = _walls.GetTilesBlock(bounds);
        TileBase[] tempFloors = _floors.GetTilesBlock(bounds);

        for (int i = 0; i < tempWalls.Length - 1; i++)
        {
            if (tempWalls[i] == null)
            {
                fullWall = false;
            }

            if (tempWalls[i] != null && tempWalls[i + 1] == null && tempFloors[i + 1] != null)
            {
                fullWall = false;
                BoundsInt chokepoint = bounds;
                for (int j = i + 2; j < tempWalls.Length; j++)
                {
                    if (tempWalls[j] != null)
                    {
                        if (horizontal)
                        {
                            chokepoint.xMin = bounds.xMin + i;
                            chokepoint.xMax = bounds.xMin + j + 1;
                        }
                        else
                        {
                            chokepoint.yMin = bounds.yMin + i;
                            chokepoint.yMax = bounds.yMin + j + 1;
                        }
                        result.Add(chokepoint);
                        i = j;
                        lockable = true;
                        break;
                    }
                    lockable = false;
                }
            }
        }

        if (fullWall)
        {
            lockable = true;
        }

        return result;
    }

    private void GeneratePools(ref Map map, in MapGeneratorParameters parameters)
    {
        _pits.SetTile(_walls.cellBounds.min, null);
        _pits.SetTile(_walls.cellBounds.max - new Vector3Int(1, 1, 1), null);

        List<BoundsInt> chokepoints = new List<BoundsInt>(map.ChokePoints);
        List<RectInt> rooms = new List<RectInt>();

        foreach(MapNode room in map.Cells)
        {
           if (_random.NextFloat() < parameters.PitFrequency)
            {
                rooms.Add(room.Cell);
            }
        }


        for (int i = 0; i < map.ChokePoints.Count; i++)
        {
            chokepoints[i] = new BoundsInt(map.ChokePoints[i].position - new Vector3Int(1, 1, 0), map.ChokePoints[i].size + new Vector3Int(2, 2, 0));
        }

        float scale = 25.0f;
        for(int x = _walls.cellBounds.xMin; x < _walls.cellBounds.xMax; x += 2)
        {
            for(int y = _walls.cellBounds.yMin; y < _walls.cellBounds.yMax; y += 2)
            {
                BoundsInt bounds = new BoundsInt(x, y, 0, 2, 2, 1);
                BoundsInt wallCheck = new BoundsInt(x - 1, y - 1, 0, 4, 4, 1);
                if (_walls.GetTilesBlock(wallCheck).Any(wall => wall != null))
                {
                    continue;
                }

                if (!rooms.Any(cell => cell.Contains(new Vector2Int(x, y))))
                {
                    continue;
                }

                if (chokepoints.Any(choke => choke.Overlaps(bounds)))
                {
                    continue;
                }

                float xCoord = _walls.cellBounds.xMin + x / (float)_walls.cellBounds.size.x * scale;
                float yCoord = _walls.cellBounds.yMin + y / (float)_walls.cellBounds.size.y * scale;

                if (Mathf.PerlinNoise(xCoord, yCoord) < 0.5f)
                {
                    _mapPainter.PaintPit(bounds.ToRectInt(), false);
                    map.UpdateCollisionMap(bounds.ToRectInt(), 2, false);
                }
            }
        }
    }
  
    private Vector2Int GenerateRandomSize(in MapGeneratorParameters parameters)
    {
        int width = _random.Range(parameters.MinCellSize, parameters.MaxCellSize);
        int height = _random.Range(parameters.MinCellSize, parameters.MaxCellSize);

        return new Vector2Int(width, height);
    }

    private Vector2Int RandomPointInCircle(float radius)
    {
        double r = radius * Math.Sqrt(_random.NextFloat());
        double theta = _random.NextFloat() * 2 * Math.PI;

        int x = (int)Math.Round(r * Math.Cos(theta));
        int y = (int)Math.Round(r * Math.Sin(theta));

        return new Vector2Int(x, y);
    }
}
