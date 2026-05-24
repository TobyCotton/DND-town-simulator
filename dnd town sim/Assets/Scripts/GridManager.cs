using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class GridManager : MonoBehaviour
{
    [SerializeField] public int m_chunksSide, m_numberOfChunksSide;
    [SerializeField] private Tile mpr_tile;
    [SerializeField] private Transform m_camTransform;
    [SerializeField] private List<BaseAI> m_aiAgentsPrefabs;

    private Vector3 m_currentHighlightedCell;
    private List<BaseAI> m_activeAIAgents;
    private Dictionary<Vector2, Dictionary<Vector2, Tile>> m_grid;

    public void SetHighlightedTile(Vector3 tilePose)
    {
        m_currentHighlightedCell = tilePose;
    }
    private void Start()
    {
        m_grid = new Dictionary<Vector2, Dictionary<Vector2, Tile>>();
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
            foreach (BaseAI ai in m_activeAIAgents)
            {
                if (ai.isSelected())
                {
                    ai.SetDestination(m_currentHighlightedCell);
                }
            }
        }
    }
    void GenerateGrid()
    {
        for (int x1 = 0; x1 < m_numberOfChunksSide; x1++)
        {
            for (int y1 = 0; y1 < m_numberOfChunksSide; y1++)
            {
                Dictionary<Vector2, Tile> chunk = new Dictionary<Vector2, Tile>();
                for (int x = 0; x < m_chunksSide; x++)
                {
                    for (int y = 0; y < m_chunksSide; y++)
                    {
                        var spawnedTile = Instantiate(mpr_tile, new Vector3(x+(x1*m_chunksSide), y + (y1 * m_chunksSide)), Quaternion.identity);
                        spawnedTile.name = $"Chunk {x1} {y1} Tile {x} {y}";

                        bool isOffSet = (x % 2 == 0 && y % 2 != 0) || (x % 2 != 0 && y % 2 == 0);
                        spawnedTile.Init(isOffSet,this);

                        chunk[new Vector2(x, y)] = spawnedTile;
                    }
                }
                m_grid[new Vector2(x1, y1)] = chunk;
            }
        }
        m_camTransform.transform.position = new Vector3((float)(m_numberOfChunksSide*m_chunksSide) / 2 - 0.5f, (float)(m_numberOfChunksSide * m_chunksSide) / 2 - 0.5f, -10);
    }

    public Tile GetTileAtPosition(Vector2 chunkPos,Vector2 pos)
    {
        if(m_grid.TryGetValue(chunkPos, out var chunk))
        {
            if (chunk.TryGetValue(pos, out var tile))
            {
                return tile;
            }
        }

        Debug.LogWarning($"Error tile not found at chunk x: {chunkPos.x} y: {chunkPos.y} position x:{pos.x} and y:{pos.y}");
        return null;
    }

    public TileType GetTileType(Vector2 chunkPos, Vector2 pos)
    {
        Tile requestedTile = GetTileAtPosition(chunkPos, pos);
        if (requestedTile != null)
        {
            return requestedTile.ThisTylesType();
        }
        return TileType.e_None;
    }
} 
