using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class BaseAI : MonoBehaviour
{
    [SerializeField] float m_speed;
    [SerializeField] GameObject m_highlight;
    private GridManager m_gridManager;

    private List<Vector3> m_path = new List<Vector3>();
    private int m_pathIndex = 0;
    private bool m_currentlyMoving = false;

    private void Start()
    {
        m_currentlyMoving = false;
    }

    private void Update()
    {
        if (!m_currentlyMoving || m_path.Count == 0)
        {
            return;
        }

        Vector3 target = m_path[m_pathIndex];

        // Get move cost at current tile to scale speed
        float cost = m_gridManager.GetMoveCostAt(transform.position);

        // Cheaper tiles = faster movement (cost 0.5 road = 2x speed)
        float speed = m_speed / cost;

        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);

        // Close enough to this waypoint — move to the next
        if (Vector3.Distance(transform.position, target) < 0.05f)
        {
            transform.position = target;
            m_pathIndex++;

            if (m_pathIndex >= m_path.Count)
            {
                // Reached the end
                m_currentlyMoving = false;
                m_path.Clear();
                m_pathIndex = 0;
            }
        }
    }
    public void SetGridManager(GridManager gridManager)
    {
        m_gridManager = gridManager;
    }
    public void SetDestination(Vector3 destination)
    {
        List<Vector3> path = BuildFullPath(transform.position, destination);

        if (path == null || path.Count == 0)
        {
            Debug.LogWarning($"{name}: No path found to {destination}");
            return;
        }

        m_path = path;
        m_pathIndex = 0;
        m_currentlyMoving = true;
        m_highlight.SetActive(false);
    }

    public void MoveToSquare(Vector3 newPos)
    {
        transform.position = newPos;
    }

    public bool isSelected()
    {
        return m_highlight.activeSelf;
    }

    void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(1))
        {
            m_highlight.SetActive(true);
        }
    }

    private List<Vector3> BuildFullPath(Vector3 startWorld, Vector3 goalWorld)
    {
        List<AbstractNode> graph = m_gridManager.GetAbstractGraph();

        m_gridManager.WorldToGrid(startWorld, out Vector2 startChunk, out Vector2 startTile);
        m_gridManager.WorldToGrid(goalWorld, out Vector2 goalChunk, out Vector2 goalTile);

        // Same chunk: skip high-level entirely
        if (startChunk == goalChunk)
        {
            return LowLevelToWorld(startChunk,
                Pathfinder.FindPathInChunk(
                    startChunk, startTile, goalTile,
                    m_gridManager, m_gridManager.m_chunksSide));
        }

        AbstractNode startNode = Pathfinder.GetNearestNode(startWorld, startChunk, graph);
        AbstractNode goalNode = Pathfinder.GetNearestNode(goalWorld, goalChunk, graph);

        List<AbstractNode> hlPath = Pathfinder.FindHighLevelPath(startNode, goalNode);

        if (hlPath == null || hlPath.Count == 0)
        {
            return null;
        }

        List<Vector3> fullPath = new List<Vector3>();

        // AI start → first waypoint
        AppendLowLevel(fullPath, startChunk, startTile, hlPath[0].tilePos);

        // Walk the high-level waypoint list
        for (int i = 0; i < hlPath.Count - 1; i++)
        {
            AbstractNode current = hlPath[i];
            AbstractNode next = hlPath[i + 1];

            if (current.chunkPos != next.chunkPos)
            {
                // Step across the border — next node is the mirror tile
                // in the neighbouring chunk, already adjacent so no low-level needed
                fullPath.Add(new Vector3(next.worldPos.x, next.worldPos.y, -1f));
            }
            else
            {
                // Both waypoints are inside the same chunk — low-level between them
                AppendLowLevel(fullPath, current.chunkPos, current.tilePos, next.tilePos);
            }
        }

        // Last waypoint → actual goal tile
        AbstractNode lastNode = hlPath[hlPath.Count - 1];
        AppendLowLevel(fullPath, goalChunk, lastNode.tilePos, goalTile);

        return fullPath;
    }

    // Runs low-level A* and appends world positions to an existing list
    private void AppendLowLevel(List<Vector3> path,Vector2 chunkPos,Vector2 fromTile,Vector2 toTile)
    {
        List<Vector2> segment = Pathfinder.FindPathInChunk(chunkPos, fromTile, toTile, m_gridManager, m_gridManager.m_chunksSide);

        if (segment == null)
        {
            return;
        }

        // Skip index 0 — it's the position we're already at
        for (int i = 1; i < segment.Count; i++)
        {
            Vector3 worldPos = Pathfinder.ChunkTileToWorld(chunkPos, segment[i], m_gridManager.m_chunksSide);
            worldPos.z = -1f;
            path.Add(worldPos);
        }
    }

    // Converts a raw low-level path (local tiles) to world positions
    private List<Vector3> LowLevelToWorld(Vector2 chunkPos, List<Vector2> tilePath)
    {
        if (tilePath == null)
        {
            return null;
        }

        List<Vector3> result = new List<Vector3>();
        for (int i = 1; i < tilePath.Count; i++)
        {
            Vector3 worldPos = Pathfinder.ChunkTileToWorld(chunkPos, tilePath[i], m_gridManager.m_chunksSide);
            worldPos.z = -1f;
            result.Add(worldPos);
        }
        return result;
    }
}
