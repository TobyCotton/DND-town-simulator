using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class GridManager : MonoBehaviour
{
    [SerializeField] private int m_width, m_height;
    [SerializeField] private Tile mpr_tile;
    [SerializeField] private Transform m_camTransform;
    [SerializeField] private List<BaseAI> m_aiAgentsPrefabs;

    private List<BaseAI> m_activeAIAgents;
    private Dictionary<Vector2, Tile> m_tiles;

    private void Start()
    {
        m_tiles = new Dictionary<Vector2, Tile>();
        GenerateGrid();
        m_activeAIAgents = new List<BaseAI>();
        for (int i =0; i < m_aiAgentsPrefabs.Count; i++)
        {
            var spawnedAi = Instantiate(m_aiAgentsPrefabs[i], new Vector3(i+1, i+1,-1), Quaternion.identity);
            spawnedAi.SetGridManager(this);
            m_activeAIAgents.Add(spawnedAi);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            bool found = false;
            for (int x = 0; x < m_width; x++)
            {
                for (int y = 0; y < m_height; y++)
                {
                    if (m_tiles[new Vector2(x, y)].isHighlighted())
                    {
                        Vector3 destination = new Vector3(x, y, 0);
                        foreach (BaseAI ai in m_activeAIAgents)
                        {
                            if(ai.isSelected())
                            {
                                ai.SetDestination(destination);
                            }
                        }
                        found = true;
                        break;
                    }
                }
                if(found)
                {
                    break;
                }
            }
        }
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

    public TileType GetTileType(Vector2 pos)
    {
        Tile requestedTile = GetTileAtPosition(pos);
        if (requestedTile != null)
        {
            return requestedTile.ThisTylesType();
        }
        return TileType.e_None;
    }
} 
