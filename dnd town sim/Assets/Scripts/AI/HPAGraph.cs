using System.Collections.Generic;
using UnityEngine;

// One crossing point between two chunks
public class AbstractNode
{
    public Vector2 chunkPos;      // Which chunk this node lives in
    public Vector2 tilePos;       // Local tile position within that chunk
    public Vector3 worldPos;      

    // Other AbstractNodes reachable from here (within the same chunk,
    // or the mirror node across a chunk border)
    public List<AbstractEdge> edges = new List<AbstractEdge>();

    public AbstractNode(Vector2 chunkPos, Vector2 tilePos, int chunkSize)
    {
        this.chunkPos = chunkPos;
        this.tilePos = tilePos;
        worldPos = Pathfinder.ChunkTileToWorld(chunkPos, tilePos, chunkSize);
    }
}

// A connection between two AbstractNodes, with a known cost
public class AbstractEdge
{
    public AbstractNode to;
    public float cost;

    public AbstractEdge(AbstractNode to, float cost)
    {
        this.to = to;
        this.cost = cost;
    }
}

public static class HPAGraphBuilder
{
    private static bool IsCrossingWalkable(Vector2 chunkA, Vector2 tileA,Vector2 chunkB, Vector2 tileB,GridManager grid)
    {
        Tile a = grid.GetTileAtPosition(chunkA, tileA);
        Tile b = grid.GetTileAtPosition(chunkB, tileB);
        return a != null && b != null && a.IsWalkable() && b.IsWalkable();
    }
    private static AbstractNode GetOrCreate(Vector2 chunkPos, Vector2 tilePos, int chunkSize,Dictionary<(Vector2, Vector2), AbstractNode> nodeMap,List<AbstractNode> nodes)
    {
        (Vector2, Vector2) key = (chunkPos, tilePos);
        if (!nodeMap.TryGetValue(key, out AbstractNode node))
        {
            node = new AbstractNode(chunkPos, tilePos, chunkSize);
            nodeMap[key] = node;
            nodes.Add(node);
        }
        return node;
    }
    private static float PathCost(List<Vector2> path, Vector2 chunkPos, GridManager grid)
    {
        float total = 0f;
        for (int i = 1; i < path.Count; i++)
        {
            Vector3 worldPos = Pathfinder.ChunkTileToWorld(chunkPos, path[i], grid.m_chunksSide);
            total += grid.GetMoveCostAt(worldPos);
        }
        return total;
    }

    private static void CheckNeighbour(GridManager grid, List<AbstractNode> nodes,
        Dictionary<(Vector2, Vector2), AbstractNode> nodeMap,
        Vector2 chunk, int numChunks, int chunkSize, int cx, int cy, bool checkRight)
    {
        int cordNumber = checkRight ? cx + 1 : cy + 1;

        if (cordNumber >= numChunks)
        {
            return;
        }

        Vector2 chunkB = checkRight ? new Vector2(cordNumber, cy) : new Vector2(cx, cy + 1);

        int runStart = -1;

        for (int t = 0; t <= chunkSize; t++)
        {
            Vector2 tileA = checkRight ? new Vector2(chunkSize - 1, t) : new Vector2(t, chunkSize - 1);
            Vector2 tileB = checkRight ? new Vector2(0, t) : new Vector2(t, 0);

            bool walkable = t < chunkSize && IsCrossingWalkable(chunk, tileA, chunkB, tileB, grid);

            if (walkable && runStart == -1)
            {
                runStart = t;
            }
            else if (!walkable && runStart != -1)
            {
                // Start of run
                Vector2 startTileA = checkRight ? new Vector2(chunkSize - 1, runStart) : new Vector2(runStart, chunkSize - 1);
                Vector2 startTileB = checkRight ? new Vector2(0, runStart) : new Vector2(runStart, 0);
                AbstractNode startNodeA = GetOrCreate(chunk, startTileA, chunkSize, nodeMap, nodes);
                AbstractNode startNodeB = GetOrCreate(chunkB, startTileB, chunkSize, nodeMap, nodes);
                startNodeA.edges.Add(new AbstractEdge(startNodeB, 1f));
                startNodeB.edges.Add(new AbstractEdge(startNodeA, 1f));

                // Midpoint
                int mid = (runStart + t - 1) / 2;
                Vector2 midTileA = checkRight ? new Vector2(chunkSize - 1, mid) : new Vector2(mid, chunkSize - 1);
                Vector2 midTileB = checkRight ? new Vector2(0, mid) : new Vector2(mid, 0);
                AbstractNode midNodeA = GetOrCreate(chunk, midTileA, chunkSize, nodeMap, nodes);
                AbstractNode midNodeB = GetOrCreate(chunkB, midTileB, chunkSize, nodeMap, nodes);
                midNodeA.edges.Add(new AbstractEdge(midNodeB, 1f));
                midNodeB.edges.Add(new AbstractEdge(midNodeA, 1f));

                // End of run
                int runEnd = t - 1;
                Vector2 endTileA = checkRight ? new Vector2(chunkSize - 1, runEnd) : new Vector2(runEnd, chunkSize - 1);
                Vector2 endTileB = checkRight ? new Vector2(0, runEnd) : new Vector2(runEnd, 0);
                AbstractNode endNodeA = GetOrCreate(chunk, endTileA, chunkSize, nodeMap, nodes);
                AbstractNode endNodeB = GetOrCreate(chunkB, endTileB, chunkSize, nodeMap, nodes);
                endNodeA.edges.Add(new AbstractEdge(endNodeB, 1f));
                endNodeB.edges.Add(new AbstractEdge(endNodeA, 1f));

                runStart = -1;
            }
        }
    }

    public static List<AbstractNode> Build(GridManager grid)
    {
        List<AbstractNode> nodes = new List<AbstractNode>();

        int numChunks = grid.m_numberOfChunksSide;
        int chunkSize = grid.m_chunksSide;

        //find all border crossings and create AbstractNodes

        // Key: (chunkPos, tilePos) so we can look up existing nodes quickly
        Dictionary<(Vector2, Vector2), AbstractNode> nodeMap = new Dictionary<(Vector2, Vector2), AbstractNode>();

        for (int cx = 0; cx < numChunks; cx++)
        {
            for (int cy = 0; cy < numChunks; cy++)
            {
                // helper function this
                Vector2 chunkA = new Vector2(cx, cy);

                CheckNeighbour(grid, nodes, nodeMap, chunkA, numChunks, chunkSize, cx, cy, true);
                CheckNeighbour(grid, nodes, nodeMap, chunkA, numChunks, chunkSize, cx, cy, false);
            }
        }

        // --- Pass 2: connect nodes within the same chunk ---
        // For each chunk, find all abstract nodes that belong to it,
        // then run low-level A* between every pair to get intra-chunk costs.

        // Group nodes by chunk
        Dictionary<Vector2, List<AbstractNode>> byChunk = new Dictionary<Vector2, List<AbstractNode>>();
        foreach (AbstractNode node in nodes)
        {
            if (!byChunk.ContainsKey(node.chunkPos))
            {
                byChunk[node.chunkPos] = new List<AbstractNode>();
            }
            byChunk[node.chunkPos].Add(node);
        }

        foreach (KeyValuePair<Vector2, List<AbstractNode>> kvp in byChunk)
        {
            Vector2 chunkPos = kvp.Key;
            List<AbstractNode> chunkNodes = kvp.Value;

            // Connect every pair within this chunk
            for (int i = 0; i < chunkNodes.Count; i++)
            {
                for (int j = i + 1; j < chunkNodes.Count; j++)
                {
                    AbstractNode a = chunkNodes[i];
                    AbstractNode b = chunkNodes[j];

                    List<Vector2> path = Pathfinder.FindPathInChunk(chunkPos, a.tilePos, b.tilePos, grid, chunkSize);

                    if (path != null)
                    {
                        float cost = PathCost(path, chunkPos, grid);
                        a.edges.Add(new AbstractEdge(b, cost));
                        b.edges.Add(new AbstractEdge(a, cost));
                    }
                }
            }
        }

        return nodes;
    }
}
