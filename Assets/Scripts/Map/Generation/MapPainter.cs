using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapPainter
{
    private TileContainer _wallContainer;
    private TileContainer _pitContainer;
    private Tilemap _floors;
    private Tilemap _walls;
    private Tilemap _pits;

    public void Initialize(TileContainer wallContainer, TileContainer pitContainer, Tilemap floors, Tilemap walls, Tilemap pits)
    {
        _wallContainer = wallContainer;
        _pitContainer = pitContainer;
        _floors = floors;
        _walls = walls;
        _pits = pits;
    }

    public void PaintRoom(RectInt cell, bool paintWalls)
    {
        Vector3Int pos = new Vector3Int(cell.xMin, cell.yMin, 0);
        Vector3Int size = new Vector3Int(cell.width, cell.height, 1);

        TileBase[] tiles = new TileBase[size.x * size.y];
        for (int i = 0; i < size.x * size.y; i++)
        {
            tiles[i] = _wallContainer.FloorTiles.GetRandom();
        }

        _floors.SetTilesBlock(new BoundsInt(pos, size), tiles);

        if (paintWalls)
        {
            for (int i = 0; i < size.x * size.y; i++)
            {
                tiles[i] = null;

                int x = i % size.x + pos.x;
                int y = i / size.x + pos.y;

                if (x == pos.x && y == pos.y)
                {
                    tiles[i] = _wallContainer.BottomLeft;
                }
                else if (x == pos.x && y == cell.yMax - 1)
                {
                    tiles[i] = _wallContainer.TopLeft;
                }
                else if (x == cell.xMax - 1 && y == pos.y)
                {
                    tiles[i] = _wallContainer.BottomRight;
                }
                else if (x == cell.xMax - 1 && y == cell.yMax - 1)
                {
                    tiles[i] = _wallContainer.TopRight;
                }
                else if (y == cell.yMax - 1)
                {
                    tiles[i] = _wallContainer.TopMiddle;
                }
                else if (y == pos.y)
                {
                    tiles[i] = _wallContainer.BottomMiddle;
                }
                else if (x == cell.xMax - 1)
                {
                    tiles[i] = _wallContainer.MiddleRight;
                }
                else if (x == pos.x)
                {
                    tiles[i] = _wallContainer.MiddleLeft;
                }
            }

            _walls.SetTilesBlock(new BoundsInt(pos, size), tiles);
        }
    }

    public void PaintPit(RectInt cell, bool paintWalls)
    {
        BoundsInt floorCheck = cell.ToBoundsInt();
        floorCheck.size += new Vector3Int(2, 2, 0);
        floorCheck.position -= new Vector3Int(1, 1, 0);

        TileBase[] floorTiles = _floors.GetTilesBlock(floorCheck);
        if (floorTiles.Any(x => x == null))
        {
            return;
        }

        Vector3Int pos = new Vector3Int(cell.xMin, cell.yMin, 0);
        Vector3Int size = new Vector3Int(cell.width, cell.height, 1);

        TileBase[] tiles = new TileBase[size.x * size.y];
        for (int i = 0; i < size.x * size.y; i++)
        {
            tiles[i] = _pitContainer.FloorTiles.GetRandom();
        }

        _pits.SetTilesBlock(new BoundsInt(pos, size), tiles);

        if (paintWalls)
        {
            for (int i = 0; i < size.x * size.y; i++)
            {
                tiles[i] = null;

                int x = i % size.x + pos.x;
                int y = i / size.x + pos.y;

                if (x == pos.x && y == pos.y)
                {
                    tiles[i] = _pitContainer.BottomLeft;
                }
                else if (x == pos.x && y == cell.yMax - 1)
                {
                    tiles[i] = _pitContainer.TopLeft;
                }
                else if (x == cell.xMax - 1 && y == pos.y)
                {
                    tiles[i] = _pitContainer.BottomRight;
                }
                else if (x == cell.xMax - 1 && y == cell.yMax - 1)
                {
                    tiles[i] = _pitContainer.TopRight;
                }
                else if (y == cell.yMax - 1)
                {
                    tiles[i] = _pitContainer.TopMiddle;
                }
                else if (y == pos.y)
                {
                    tiles[i] = _pitContainer.BottomMiddle;
                }
                else if (x == cell.xMax - 1)
                {
                    tiles[i] = _pitContainer.MiddleRight;
                }
                else if (x == pos.x)
                {
                    tiles[i] = _pitContainer.MiddleLeft;
                }
            }

            _pits.SetTilesBlock(new BoundsInt(pos, size), tiles);
        }
    }

    public void PaintTiles(in Map map, in MapGeneratorParameters parameters)
    {
        for (int boundsY = 0; boundsY < map.Bounds.size.y; boundsY++)
        {
            for (int boundsX = 0; boundsX < map.Bounds.size.x; boundsX++)
            {
                int x = map.Bounds.xMin + boundsX;
                int y = map.Bounds.yMin + boundsY;

                RemoveThinWalls(x, y);

                Tile tile = GetTileByNeighbours(x, y);
                if (tile == null)
                {
                    continue;
                }

                map.CollisionMap[boundsX, boundsY] = 1;
                _walls.SetTile(new Vector3Int(x, y, 0), tile);
            }
        }
    }

    private bool RemoveThinWalls(int x, int y)
    {
        Vector3Int pos = new Vector3Int(x, y, 0);

        Tile middleRightWall = (Tile)_walls.GetTile(pos + new Vector3Int(1, 0, 0));
        Tile middleLeftWall = (Tile)_walls.GetTile(pos - new Vector3Int(1, 0, 0));
        Tile middleTopWall = (Tile)_walls.GetTile(pos + new Vector3Int(0, 1, 0));
        Tile middleBottomWall = (Tile)_walls.GetTile(pos - new Vector3Int(0, 1, 0));

        Tile middleLeftFloor = (Tile)_floors.GetTile(pos - new Vector3Int(1, 0, 0));
        Tile middleRightFloor = (Tile)_floors.GetTile(pos + new Vector3Int(1, 0, 0));
        Tile middleTopFloor = (Tile)_floors.GetTile(pos + new Vector3Int(0, 1, 0));
        Tile middleBottomFloor = (Tile)_floors.GetTile(pos - new Vector3Int(0, 1, 0));

        if (middleLeftFloor != null && middleRightFloor != null && middleRightWall == null && middleLeftWall == null)
        {
            _walls.SetTile(pos, null);
            return true;
        }

        if (middleTopFloor != null && middleBottomFloor != null && middleTopWall == null && middleBottomWall == null)
        {
            _walls.SetTile(pos, null);
            return true;
        }

        return false;
    }
    private Tile GetTileByNeighbours(int x, int y)
    {
        TileBase currentFloorTile = _floors.GetTile(new Vector3Int(x, y, 0));

        if (currentFloorTile == null)
        {
            return null;
        }

        BoundsInt blockBounds = new BoundsInt(x - 1, y - 1, 0, 3, 3, 1);
        TileBase[] floorBlock = _floors.GetTilesBlock(blockBounds);

        Tile floorBottomLeft = (Tile)floorBlock[0];
        Tile floorBottomMiddle = (Tile)floorBlock[1];
        Tile floorBottomRight = (Tile)floorBlock[2];
        Tile floorMiddleLeft = (Tile)floorBlock[3];
        Tile floorMiddleRight = (Tile)floorBlock[5];
        Tile floorTopLeft = (Tile)floorBlock[6];
        Tile floorTopMiddle = (Tile)floorBlock[7];
        Tile floorTopRight = (Tile)floorBlock[8];

        if (floorBottomLeft == null)
        {
            if (floorMiddleLeft == null && floorBottomMiddle == null)
            {
                return _wallContainer.BottomLeftOuter;
            }
            else if (floorTopRight != null && floorMiddleLeft != null && floorBottomMiddle != null)
            {
                return _wallContainer.TopRight;
            }
        }

        if (floorBottomRight == null)
        {
            if (floorMiddleRight == null && floorBottomMiddle == null)
            {
                return _wallContainer.BottomRightOuter;
            }
            else if (floorTopLeft != null && floorMiddleRight != null && floorBottomMiddle != null)
            {
                return _wallContainer.TopLeft;
            }
        }

        if (floorTopLeft == null)
        {
            if (floorMiddleLeft == null && floorTopMiddle == null)
            {
                return _wallContainer.TopLeftOuter;
            }
            else if (floorMiddleLeft != null && floorTopMiddle != null)
            {
                return _wallContainer.BottomRight;
            }
        }

        if (floorTopRight == null)
        {
            if (floorMiddleRight == null && floorTopMiddle == null)
            {
                return _wallContainer.TopRightOuter;
            }
            else if (floorMiddleRight != null && floorTopMiddle != null)
            {
                return _wallContainer.BottomLeft;
            }
        }

        if (floorMiddleLeft != null && floorMiddleRight != null)
        {
            if (floorTopMiddle == null)
            {
                return _wallContainer.TopMiddle;
            }
            else if (floorBottomMiddle == null)
            {
                return _wallContainer.BottomMiddle;
            }
        }

        if (floorTopMiddle != null && floorBottomMiddle != null)
        {
            if (floorMiddleRight == null)
            {
                return _wallContainer.MiddleRight;
            }
            else if (floorMiddleLeft == null)
            {
                return _wallContainer.MiddleLeft;
            }
        }
  
        return null;
    }

    public void PostProcessTiles(in Map map, in MapGeneratorParameters parameters)
    {
        for (int x = map.Bounds.xMin; x < map.Bounds.xMax; x++)
        {
            for (int y = map.Bounds.yMin; y < map.Bounds.yMax; y++)
            {

                Vector3Int pos = new Vector3Int(x, y, 0);
                Tile currentPitTile = (Tile)_pits.GetTile(pos);

                if (currentPitTile == null)
                {
                    continue;
                }

                BoundsInt blockBounds = new BoundsInt(x - 1, y - 1, 0, 3, 3, 1);
                
                if (currentPitTile != null)
                {
                    PostProcessPits(x, y, blockBounds);
                }
            }
        }
    }

    private void PostProcessPits(int x, int y, BoundsInt blockBounds)
    {
        TileBase[] pitBlock = _pits.GetTilesBlock(blockBounds);

        Tile bottomLeft = (Tile)pitBlock[0];
        Tile bottomMid = (Tile)pitBlock[1];
        Tile bottomRight = (Tile)pitBlock[2];
        Tile middleLeft = (Tile)pitBlock[3];
        Tile middleRight = (Tile)pitBlock[5];
        Tile topLeft = (Tile)pitBlock[6];
        Tile topMid = (Tile)pitBlock[7];
        Tile topRight = (Tile)pitBlock[8];

        if (topMid == null && middleRight != null && bottomMid != null && middleLeft == null)
        {
            _pits.SetTile(new Vector3Int(x, y, 0), _pitContainer.TopLeft);
            return;
        }

        if (topMid == null && middleRight == null && bottomMid != null && middleLeft != null)
        {
            _pits.SetTile(new Vector3Int(x, y, 0), _pitContainer.TopRight);
            return;
        }

        if (middleLeft == null && bottomMid == null && topMid != null && middleRight != null)
        {
            _pits.SetTile(new Vector3Int(x, y, 0), _pitContainer.BottomLeft);
            return;
        }

        if (middleLeft != null && bottomMid == null && topMid != null && middleRight == null)
        {
            _pits.SetTile(new Vector3Int(x, y, 0), _pitContainer.BottomRight);
            return;
        }

        if (middleLeft != null && middleRight != null && topMid == null && bottomMid != null)
        {
            _pits.SetTile(new Vector3Int(x, y, 0), _pitContainer.TopMiddle);
            return;
        }

        if (middleLeft != null && middleRight != null && topMid != null && bottomMid == null)
        {
            _pits.SetTile(new Vector3Int(x, y, 0), _pitContainer.BottomMiddle);
            return;
        }

        if (middleLeft != null && middleRight == null && topMid != null && bottomMid != null)
        {
            _pits.SetTile(new Vector3Int(x, y, 0), _pitContainer.MiddleRight);
            return;
        }

        if (middleLeft == null && middleRight != null && topMid != null && bottomMid != null)
        {
            _pits.SetTile(new Vector3Int(x, y, 0), _pitContainer.MiddleLeft);
            return;
        }

        if (bottomLeft == null && middleLeft != null && bottomMid != null)
        {
            _pits.SetTile(new Vector3Int(x, y, 0), _pitContainer.BottomLeftOuter);
            return;
        }

        if (bottomRight == null && middleRight != null && bottomMid != null)
        {
            _pits.SetTile(new Vector3Int(x, y, 0), _pitContainer.BottomRightOuter);
            return;
        }

        if (topLeft == null && middleLeft != null && topMid != null)
        {
            _pits.SetTile(new Vector3Int(x, y, 0), _pitContainer.TopLeftOuter);
            return;
        }

        if (topRight == null && middleRight != null && topMid != null)
        {
            _pits.SetTile(new Vector3Int(x, y, 0), _pitContainer.TopRightOuter);
            return;
        }
    }

    public void BuildVerticalDoorWalls(Map map, MapNode target, BoundsInt chokepoint)
    {
        BoundsInt bounds = chokepoint;
        bounds.yMin += 3;
        BoundsInt drawArea = new BoundsInt(bounds.position, bounds.size);

        //Chokepoint is on the left edge
        if (chokepoint.center.x < target.Cell.center.x)
        {
            drawArea.xMin -= 1;
            PainVerticalWall(drawArea.ToRectInt(), true);
        }
        //chokepoint is on the right edge
        else
        {
            drawArea.xMax += 1;
            PainVerticalWall(drawArea.ToRectInt(), false);
        }

        map.UpdateCollisionMap(drawArea.ToRectInt(), 1);
    }

    public void BuildHorizontalDoorWalls(Map map, MapNode target, BoundsInt chokepoint)
    {
        BoundsInt bounds = new BoundsInt(chokepoint.position, chokepoint.size);
        bounds.xMin += 3;
        BoundsInt drawArea = new BoundsInt(bounds.position, bounds.size);

        //Chokepoint is on the lower edge
        if (chokepoint.center.y < target.Cell.center.y)
        {
            drawArea.yMin -= 1;
            PaintHorizontalWall(drawArea.ToRectInt(), false);

        }
        //chokepoint is on the upper edge
        else
        {
            drawArea.yMax += 1;
            PaintHorizontalWall(drawArea.ToRectInt(), true);
        }

        map.UpdateCollisionMap(drawArea.ToRectInt(), 1);
    }

    private void PainVerticalWall(RectInt cell, bool left)
    {
        Vector3Int pos = new Vector3Int(cell.xMin, cell.yMin, 0);
        Vector3Int size = new Vector3Int(cell.width, cell.height, 1);

        TileBase[] tiles = new TileBase[size.x * size.y];

        for (int i = 0; i < size.x * size.y; i++)
        {
            tiles[i] = null;

            int x = i % size.x + pos.x;
            int y = i / size.x + pos.y;

            Tile floorTop = (Tile)_floors.GetTile(new Vector3Int(x, y + 1, 0));

            if (x == pos.x && y == pos.y)
            {
                tiles[i] = _wallContainer.BottomLeft;
            }
            else if (x == pos.x && y == cell.yMax - 1)
            {
                if (left)
                {
                    tiles[i] = _wallContainer.TopRightOuter;
                }
                else
                {
                    tiles[i] = floorTop == null ? _wallContainer.TopRightOuter : _wallContainer.MiddleRight;
                }
            }
            else if (x == cell.xMax - 1 && y == pos.y)
            {
                tiles[i] = _wallContainer.BottomRight;
            }
            else if (x == cell.xMax - 1 && y == cell.yMax - 1)
            {
                if (left)
                {
                    tiles[i] = floorTop == null ? _wallContainer.TopLeftOuter : _wallContainer.MiddleLeft;
                }
                else
                {
                    tiles[i] = _wallContainer.TopLeftOuter;
                }
            }
            else if (x == cell.xMax - 1)
            {
                tiles[i] = _wallContainer.MiddleLeft;
            }
            else if (x == pos.x)
            {
                tiles[i] = _wallContainer.MiddleRight;
            }
        }

        _walls.SetTilesBlock(new BoundsInt(pos, size), tiles);
    }

    private void PaintHorizontalWall(RectInt cell, bool top)
    {
        Vector3Int pos = new Vector3Int(cell.xMin, cell.yMin, 0);
        Vector3Int size = new Vector3Int(cell.width, cell.height, 1);

        TileBase[] tiles = new TileBase[size.x * size.y];

        for (int i = 0; i < size.x * size.y; i++)
        {
            tiles[i] = null;

            int x = i % size.x + pos.x;
            int y = i / size.x + pos.y;

            Tile floorRight = (Tile)_floors.GetTile(new Vector3Int(x + 1, y, 0));

            if (x == pos.x && y == pos.y)
            {
                tiles[i] = _wallContainer.BottomLeft;
            }
            else if (x == pos.x && y == cell.yMax - 1)
            {
                tiles[i] = _wallContainer.TopLeft;
            }
            else if (x == cell.xMax - 1 && y == pos.y)
            {
                if (top)
                {
                    tiles[i] = _wallContainer.TopMiddle;
                }
                else
                {
                    tiles[i] = floorRight == null ? _wallContainer.TopRightOuter : _wallContainer.TopMiddle;
                }
            }
            else if (x == cell.xMax - 1 && y == cell.yMax - 1)
            {
                if (top)
                {
                    tiles[i] = floorRight == null ? _wallContainer.BottomRightOuter : _wallContainer.BottomMiddle;
                }
                else
                {
                    tiles[i] = floorRight == null ? _wallContainer.BottomRightOuter : _wallContainer.BottomMiddle;
                }
            }
            else if (y == cell.yMax - 1)
            {
                tiles[i] = _wallContainer.BottomMiddle;
            }
            else if (y == pos.y)
            {
                tiles[i] = _wallContainer.TopMiddle;
            }
        }

        _walls.SetTilesBlock(new BoundsInt(pos, size), tiles);
    }
}
