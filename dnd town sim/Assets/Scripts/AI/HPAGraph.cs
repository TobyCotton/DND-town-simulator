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

    private static void CheckNeighbour(GridManager grid, List<AbstractNode> nodes, Dictionary<(Vector2, Vector2), AbstractNode> nodeMap, 
        Vector2 chunk,int numChunks, int chunkSize, int cx, int cy, bool checkRight)
    {
        int cordNumber = checkRight ? cx + 1 : cy + 1;
        
        if (cordNumber < numChunks)
        {
            Vector2 chunkB = checkRight ? new Vector2(cordNumber, cy) : new Vector2(cx, cy + 1);
            // The border column: rightmost column of chunkA, leftmost of chunkB
            for (int t = 0; t < chunkSize; t++)
            {
                Vector2 tileA = checkRight ? new Vector2(chunkSize - 1, t) : new Vector2(t, chunkSize - 1);
                Vector2 tileB = checkRight ? new Vector2(0, t) : new Vector2(t, 0);

                if (IsCrossingWalkable(chunk, tileA, chunkB, tileB, grid))
                {
                    AbstractNode nodeA = GetOrCreate(chunk, tileA, chunkSize, nodeMap, nodes);
                    AbstractNode nodeB = GetOrCreate(chunkB, tileB, chunkSize, nodeMap, nodes);

                    // Cross-chunk edges cost 1 (stepping across the border)
                    nodeA.edges.Add(new AbstractEdge(nodeB, 1f));
                    nodeB.edges.Add(new AbstractEdge(nodeA, 1f));
                }
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

        foreach (var kvp in byChunk)
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
