using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [SerializeField] public int m_chunksSide, m_numberOfChunksSide;
    [SerializeField] private Tile mpr_tile;
    [SerializeField] private Transform m_camTransform;
    [SerializeField] private List<BaseAI> m_aiAgentsPrefabs;

    private Vector3 m_currentHighlightedCell;
    private List<BaseAI> m_activeAIAgents;
    private Dictionary<Vector2, Dictionary<Vector2, Tile>> m_grid;
    private List<AbstractNode> m_abstractGraph;

    private static string SavePath => System.IO.Path.Combine(Application.persistentDataPath, "mapSave.json");
    public List<AbstractNode> GetAbstractGraph()
    {
        return m_abstractGraph;
    }
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
            BaseAI spawnedAi = Instantiate(m_aiAgentsPrefabs[i], new Vector3(i+1, i+1,-1), Quaternion.identity);
            spawnedAi.SetGridManager(this);
            m_activeAIAgents.Add(spawnedAi);
        }
        LoadMap();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            m_abstractGraph = HPAGraphBuilder.Build(this);
            foreach (BaseAI ai in m_activeAIAgents)
            {
                if (ai.isSelected())
                {
                    ai.SetDestination(m_currentHighlightedCell);
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.F5))
        {
            SaveMap();
        }
        if (Input.GetKeyDown(KeyCode.F1))
        {
            BaseAI spawnedAi = Instantiate(m_aiAgentsPrefabs[0], m_currentHighlightedCell, Quaternion.identity);
            spawnedAi.SetGridManager(this);
            m_activeAIAgents.Add(spawnedAi);
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
        if(m_grid.TryGetValue(chunkPos, out Dictionary<Vector2, Tile> chunk))
        {
            if (chunk.TryGetValue(pos, out Tile tile))
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

    
    public bool WorldToGrid(Vector3 worldPos, out Vector2 chunkPos, out Vector2 tilePos)// Converts a world position to chunk and tile coordinates
    {
        int tileX = Mathf.FloorToInt(worldPos.x);
        int tileY = Mathf.FloorToInt(worldPos.y);

        chunkPos = new Vector2(Mathf.FloorToInt((float)tileX / m_chunksSide),Mathf.FloorToInt((float)tileY / m_chunksSide));

        // Local position within the chunk
        tilePos = new Vector2(tileX - (int)chunkPos.x * m_chunksSide,tileY - (int)chunkPos.y * m_chunksSide);

        // Validate it's within bounds
        return tileX >= 0 && tileY >= 0 && chunkPos.x < m_numberOfChunksSide && chunkPos.y < m_numberOfChunksSide;
    }

    public Vector2 WorldToGridChunk(Vector3 worldPos)
    {
        WorldToGrid(worldPos, out Vector2 chunkPos, out Vector2 _);
        return chunkPos;
    }

    public Vector2 WorldToGridTile(Vector3 worldPos)
    {
        WorldToGrid(worldPos, out Vector2 _, out Vector2 tilePos);
        return tilePos;
    }


    public bool IsWalkable(Vector3 worldPos)
    {
        if (!WorldToGrid(worldPos, out Vector2 chunkPos, out Vector2 tilePos))
        {
            return false;
        }

        Tile tile = GetTileAtPosition(chunkPos, tilePos);
        if (tile == null)
        {
            return false;
        }
        return tile.IsWalkable();
    }

    public float GetMoveCostAt(Vector3 worldPos)
    {
        if (!WorldToGrid(worldPos, out Vector2 chunkPos, out Vector2 tilePos))
        {
            return float.MaxValue;
        }

        Tile tile = GetTileAtPosition(chunkPos, tilePos);
        return tile != null ? tile.GetMoveCost() : float.MaxValue;
    }
    public bool TryPlaceBuilding(Vector2 chunkPos, Vector2 tilePos, int width = 3, int height = 3)
    {
        Vector2 topLeft = new Vector2(tilePos.x - width / 2, tilePos.y - height / 2);

        // Pass 1: validate all tiles are clear
        for (int dx = 0; dx < width; dx++)
        {
            for (int dy = 0; dy < height; dy++)
            {
                Vector2 resolvedChunk;
                Vector2 resolvedTile;

                bool inBounds = WorldTileToChunkTile(
                    chunkPos, topLeft + new Vector2(dx, dy),
                    out resolvedChunk, out resolvedTile
                );

                if (!inBounds)
                {
                    Debug.LogWarning($"TryPlaceBuilding failed: offset ({dx},{dy}) is outside grid bounds");
                    return false;
                }

                Tile tile = GetTileAtPosition(resolvedChunk, resolvedTile);

                if (tile == null)
                {
                    Debug.LogWarning($"TryPlaceBuilding failed: null tile at chunk {resolvedChunk} tile {resolvedTile}");
                    return false;
                }
                if (!tile.IsWalkable())
                {
                    Debug.LogWarning($"TryPlaceBuilding failed: unwalkable tile at chunk {resolvedChunk} tile {resolvedTile}");
                    return false;
                }
                if (IsTileOccupiedByAgent(resolvedChunk, resolvedTile))
                {
                    Debug.LogWarning($"TryPlaceBuilding failed: agent occupying tile at chunk {resolvedChunk} tile {resolvedTile}");
                    return false;
                }
            }
        }

        // Pass 2: occupy
        BuildingData m_data = new BuildingData
        {
            m_originChunk = chunkPos,
            m_originTile = tilePos,
            m_width = width,
            m_height = height
        };

        for (int dx = 0; dx < width; dx++)
        {
            for (int dy = 0; dy < height; dy++)
            {
                Vector2 resolvedChunk;
                Vector2 resolvedTile;

                WorldTileToChunkTile(
                    chunkPos, topLeft + new Vector2(dx, dy),
                    out resolvedChunk, out resolvedTile
                );

                Tile tile = GetTileAtPosition(resolvedChunk, resolvedTile);
                tile.OccupyAsBuilding(m_data, isOrigin: dx == width / 2 && dy == height / 2);
            }
        }

        return true;
    }

    public void SaveMap()
    {
        MapSaveData m_saveData = new MapSaveData();

        foreach (KeyValuePair<Vector2, Dictionary<Vector2, Tile>> chunkKvp in m_grid)
        {
            Vector2 chunkPos = chunkKvp.Key;
            foreach (KeyValuePair<Vector2, Tile> tileKvp in chunkKvp.Value)
            {
                Tile tile = tileKvp.Value;
                TileType type = tile.ThisTylesType();

                if (type == TileType.e_None) continue;
                if (type == TileType.e_Building && !tile.IsBuildingOrigin()) continue;

                m_saveData.m_tiles.Add(tile.GetSaveData(chunkPos, tileKvp.Key));
            }
        }

        System.IO.File.WriteAllText(SavePath, JsonUtility.ToJson(m_saveData, true));
        Debug.Log($"Saved {m_saveData.m_tiles.Count} tiles to {SavePath}");
    }

    public void LoadMap()
    {
        if (!System.IO.File.Exists(SavePath))
        {
            Debug.LogWarning("No save file found.");
            return;
        }

        MapSaveData m_saveData = JsonUtility.FromJson<MapSaveData>(System.IO.File.ReadAllText(SavePath));

        foreach (TileSaveData td in m_saveData.m_tiles)
        {
            Vector2 chunkPos = new Vector2(td.m_chunkX, td.m_chunkY);
            Vector2 tilePos = new Vector2(td.m_tileX, td.m_tileY);

            if (td.m_isBuildingOrigin)
            {
                TryPlaceBuilding(chunkPos, tilePos, td.m_buildingWidth, td.m_buildingHeight);
            }
            else
            {
                GetTileAtPosition(chunkPos, tilePos)?.LoadTileType(td.m_tileType);
            }
        }

        Debug.Log($"Loaded {m_saveData.m_tiles.Count} tiles.");
    }

    private bool WorldTileToChunkTile(Vector2 chunkPos, Vector2 tilePos,
    out Vector2 resolvedChunk, out Vector2 resolvedTile)
    {
        // Convert to absolute world tile coords
        int absX = (int)(chunkPos.x * m_chunksSide + tilePos.x);
        int absY = (int)(chunkPos.y * m_chunksSide + tilePos.y);

        // Rederive chunk and local tile from absolute coords
        resolvedChunk = new Vector2(Mathf.FloorToInt((float)absX / m_chunksSide),Mathf.FloorToInt((float)absY / m_chunksSide));
        resolvedTile = new Vector2(absX - (int)resolvedChunk.x * m_chunksSide,absY - (int)resolvedChunk.y * m_chunksSide);

        // Validate it's within the overall grid bounds
        return resolvedChunk.x >= 0 && resolvedChunk.y >= 0 && resolvedChunk.x < m_numberOfChunksSide && resolvedChunk.y < m_numberOfChunksSide;
    }
    private bool IsTileOccupiedByAgent(Vector2 resolvedChunk, Vector2 resolvedTile)
    {
        if (m_activeAIAgents == null) return false;

        foreach (BaseAI agent in m_activeAIAgents)
        {
            if (agent == null) continue;

            WorldToGrid(agent.transform.position, out Vector2 agentChunk, out Vector2 agentTile);

            if (agentChunk == resolvedChunk && agentTile == resolvedTile)
            {
                return true;
            }
        }
        return false;
    }
} 
