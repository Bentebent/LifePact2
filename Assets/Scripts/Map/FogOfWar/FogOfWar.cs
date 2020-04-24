using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[System.Flags]
public enum TileType
{
    None = 0,
    Floor = 1,
    Wall = 2,
}

public class FogOfWar
{
    private MaterialPropertyBlock _mpb = null;
    private Texture2D _texture = null;
    private int _textureID = 0;

    private Vector3Int _size = Vector3Int.zero;
    private Vector3Int _origin = Vector3Int.zero;
    private TileType[,] _tiles = new TileType[0, 0];

    public FogOfWar()
    {
        _mpb = new MaterialPropertyBlock();
        _textureID = Shader.PropertyToID("_MainTex");
    }

    public void GenerateTexture(in Tilemap floor, in Tilemap walls, ref SpriteRenderer spriteRenderer)
    {
        _size = new Vector3Int(Mathf.Max(floor.size.x, walls.size.x), Mathf.Max(floor.size.y, walls.size.y), 0);
        _origin = new Vector3Int(Mathf.Min(floor.origin.x, walls.origin.x), Mathf.Min(floor.origin.y, walls.origin.y), 0);
        _texture = new Texture2D(_size.x, _size.y, TextureFormat.RGBA32, false, false);
        _texture.wrapMode = TextureWrapMode.Clamp;
        _texture.filterMode = FilterMode.Point;

        _tiles = new TileType[_size.x, _size.y];
        for (int y = 0; y < _size.y; y++)
        {
            for (int x = 0; x < _size.x; x++)
            {
                Vector3Int position = new Vector3Int(x, y, 0);
                _tiles[x, y] = TileType.None;

                if (floor.HasTile(position + _origin))
                {
                    _tiles[x, y] |= TileType.Floor;
                }

                if (walls.HasTile(position + _origin))
                {
                    _tiles[x, y] |= TileType.Wall;
                }
            }
        }

        Color32 resetColor = new Color32(0, 0, 0, 0);
        Color32[] resetColorArray = _texture.GetPixels32();

        for (int i = 0; i < resetColorArray.Length; i++)
        {
            resetColorArray[i] = resetColor;
        }

        _texture.SetPixels32(resetColorArray);

        spriteRenderer.transform.localScale = Vector3.one;
        spriteRenderer.transform.position = _origin;

        Sprite sprite = Sprite.Create(_texture, new Rect(Vector2.zero, new Vector2(_size.x, _size.y)), Vector2.zero, 1.0f);
        spriteRenderer.sprite = sprite;

        SetTexture(ref spriteRenderer);
    }

    public void UpdateFogOfWar(Vector3 playerPosition, float viewRange, ref SpriteRenderer spriteRenderer)
    {
        Vector3Int tilePosition = WorldToTile(playerPosition - new Vector3(0.5f, 0.5f, 0f));
        int intViewRange = (int)viewRange + 1;
        BoundsInt fowBounds = new BoundsInt(tilePosition - new Vector3Int(intViewRange, intViewRange, 0), new Vector3Int(2 * intViewRange + 1, 2 * intViewRange + 1, 0));

        float[,] visibility = new float[fowBounds.size.x, fowBounds.size.y];
        ShadowCast.CalculateVisibility(playerPosition - _origin, fowBounds, _tiles, visibility);

        for (int x = fowBounds.xMin; x < fowBounds.xMax; x++)
        {
            if (x < 0 || x >= _size.x)
            {
                continue;
            }

            for (int y = fowBounds.yMin; y < fowBounds.yMax; y++)
            {
                if (y < 0 || y >= _size.y)
                {
                    continue;
                }

                Vector3Int pos = new Vector3Int(x, y, 0);
                TileType tileType = _tiles[x, y];

                float distance = Vector3.Distance(playerPosition - new Vector3(0.5f, 0.5f, 0f), TileToWorld(pos));
                Color pixelColor = _texture.GetPixel(pos.x, pos.y);
                Color targetColor = pixelColor;
                float fuzzRange = 5f;
                float vis = visibility[x - fowBounds.xMin, y - fowBounds.yMin];
                if (distance < viewRange - fuzzRange)
                {
                    targetColor.r = 1f;
                }
                else if (distance > viewRange)
                {
                    targetColor.r = 0f;
                }
                else
                {
                    float frac = (viewRange - distance) / fuzzRange;
                    targetColor.r = frac;
                }

                targetColor.r *= vis;
                targetColor.g = Mathf.Max(targetColor.g, targetColor.r);

                if (targetColor.r > 0f)
                {
                    if (x == tilePosition.x && y == tilePosition.y)
                    {
                        targetColor.b = 1.0f;
                    }
                    else if (tileType.HasFlag(TileType.Wall))
                    {
                        targetColor.b = 0.75f;
                    }
                    else if (tileType.HasFlag(TileType.Floor))
                    {
                        targetColor.b = 0.5f;
                    }
                }

                _texture.SetPixel(pos.x, pos.y, targetColor);
            }
        }


        SetTexture(ref spriteRenderer);
    }

    private Vector3Int WorldToTile(Vector3 worldPos)
    {
        return worldPos.ToVector3Int() - _origin;
    }

    private Vector3 TileToWorld(Vector3Int tilePos)
    {
        return tilePos + _origin;
    }

    private void SetTexture(ref SpriteRenderer spriteRenderer)
    {
        _texture.Apply();
        spriteRenderer.GetPropertyBlock(_mpb);
        _mpb.SetTexture(_textureID, _texture);
        spriteRenderer.SetPropertyBlock(_mpb);
    }

}
