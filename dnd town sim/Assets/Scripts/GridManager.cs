using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [SerializeField] private int m_width, m_height;
    [SerializeField] private Tile mpr_tile;
    [SerializeField] private Transform m_camTransform;

    private Dictionary<Vector2, Tile> m_tiles;

    private void Start()
    {
        m_tiles = new Dictionary<Vector2, Tile>();
        GenerateGrid();
    }
    void GenerateGrid()
    {
        for (int x = 0; x < m_width; x++)
        {
            for(int y = 0; y < m_height; y++)
            {
                var spawnedTile = Instantiate(mpr_tile, new Vector3(x, y), Quaternion.identity);
                spawnedTile.name = $"Tile {x} {y}";

                bool isOffSet = (x % 2 == 0 && y% 2 != 0) || (x % 2 != 0 && y % 2 == 0);
                spawnedTile.Init(isOffSet);

                m_tiles[new Vector2(x, y)] = spawnedTile;
            }
        }

        m_camTransform.transform.position = new Vector3((float)m_width / 2 - 0.5f, (float)m_height / 2 -0.5f,-10);
    }

    public Tile GetTileAtPosition(Vector2 pos)
    {
        if(m_tiles.TryGetValue(pos,out var tile))
        {
            return tile;
        }

        Debug.LogWarning($"Error tile not found at position x:{pos.x} and y:{pos.y}");
        return null;
    }
} 
