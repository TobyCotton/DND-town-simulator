using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinder
{
    // Represents one tile during the search
    private class Node
    {
        public Vector2 tilePos;   // Local position within the chunk
        public Node parent;
        public float g;           // current actual cost
        public float h;           // estimated cost from here to goal location
        public float f => g + h;  // Total estimated cost (controls expansion location for path finding)

        public Node(Vector2 tilePos, Node parent, float g, float h)
        {
            this.tilePos = tilePos;
            this.parent = parent;
            this.g = g;
            this.h = h;
        }
    }
    private static readonly Vector2[] s_neighbours = new Vector2[]{Vector2.up, Vector2.down, Vector2.left, Vector2.right};

    private static float Heuristic(Vector2 a, Vector2 b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    private static Node GetLowestF(Dictionary<Vector2, Node> open)
    {
        Node best = null;
        foreach (var node in open.Values)
        {
            if (best == null || node.f < best.f)
            { 
                best = node; 
            }
        }
        return best;
    }

    private static List<Vector2> ReconstructPath(Node goalNode)
    {
        List<Vector2> path = new List<Vector2>();
        Node current = goalNode;
        while (current != null)
        {
            path.Add(current.tilePos);
            current = current.parent;
        }
        path.Reverse();
        return path;
    }
    private static bool IsInsideChunk(Vector2 pos, int chunkSize)
    {
        return pos.x >= 0 && pos.y >= 0 && pos.x < chunkSize && pos.y < chunkSize;
    }
    private static bool IsTileWalkable(Vector2 chunkPos, Vector2 tilePos, GridManager grid)
    {
        Tile tile = grid.GetTileAtPosition(chunkPos, tilePos);
        if (tile == null)
        {
            return false;
        }
        return tile.IsWalkable();
    }

    public static Vector3 ChunkTileToWorld(Vector2 chunkPos, Vector2 tilePos, int chunkSize)
    {
        return new Vector3(tilePos.x + chunkPos.x * chunkSize,tilePos.y + chunkPos.y * chunkSize,-1f);
    }
    public static List<Vector2> FindPathInChunk(Vector2 chunkPos,Vector2 startTile,Vector2 goalTile,GridManager grid,int chunkSize)
    {
        //if goal is unwalkable there's nothing to find
        if (!IsTileWalkable(chunkPos, goalTile, grid))
        {
            return null;
        }

        //tiles to evaluate, keyed by tilePos for fast lookup
        Dictionary<Vector2, Node> open = new Dictionary<Vector2, Node>();
        HashSet<Vector2> closed = new HashSet<Vector2>();

        Node startNode = new Node(startTile, null, 0f, Heuristic(startTile, goalTile));
        open[startTile] = startNode;

        while (open.Count > 0)
        {
            // Pick the open node with the lowest f score
            Node current = GetLowestF(open);

            if (current.tilePos == goalTile)
            {
                return ReconstructPath(current);
            }

            open.Remove(current.tilePos);
            closed.Add(current.tilePos);

            foreach (Vector2 dir in s_neighbours)
            {
                Vector2 neighbourPos = current.tilePos + dir;

                // Skip if already evaluated
                if (closed.Contains(neighbourPos))
                {
                    continue;
                }

                // Skip if outside this chunk's bounds
                if (!IsInsideChunk(neighbourPos, chunkSize))
                {
                    continue;
                }

                // Skip if the tile is a wall
                if (!IsTileWalkable(chunkPos, neighbourPos, grid))
                {
                    continue;
                }

                float moveCost = grid.GetMoveCostAt(ChunkTileToWorld(chunkPos, neighbourPos, grid.m_chunksSide));

                float tentativeG = current.g + moveCost;

                if (open.TryGetValue(neighbourPos, out Node existing))
                {
                    // We found a cheaper route to an already-open node
                    if (tentativeG < existing.g)
                    {
                        existing.g = tentativeG;
                        existing.parent = current;
                    }
                }
                else
                {
                    // Brand new node
                    float h = Heuristic(neighbourPos, goalTile);
                    open[neighbourPos] = new Node(neighbourPos, current, tentativeG, h);
                }
            }
        }

        // No path found
        return null;
    }
    private class HLNode
    {
        public AbstractNode abstractNode;
        public HLNode parent;
        public float g;
        public float h;
        public float f => g + h;

        public HLNode(AbstractNode abstractNode, HLNode parent, float g, float h)
        {
            this.abstractNode = abstractNode;
            this.parent = parent;
            this.g = g;
            this.h = h;
        }
    }

    private static float HLHeuristic(AbstractNode a, AbstractNode b)
    {
        return Mathf.Abs(a.worldPos.x - b.worldPos.x) + Mathf.Abs(a.worldPos.y - b.worldPos.y);
    }

    private static HLNode GetLowestFHL(Dictionary<AbstractNode, HLNode> open)
    {
        HLNode best = null;
        foreach (var node in open.Values)
        {
            if (best == null || node.f < best.f)
            {
                best = node;
            }
        }
        return best;
    }

    private static List<AbstractNode> ReconstructHLPath(HLNode goalNode)
    {
        var path = new List<AbstractNode>();
        HLNode current = goalNode;
        while (current != null)
        {
            path.Add(current.abstractNode);
            current = current.parent;
        }
        path.Reverse();
        return path;
    }
    public static List<AbstractNode> FindHighLevelPath(AbstractNode startNode,AbstractNode goalNode)
    {
        if (startNode == null || goalNode == null)
        {
            return null;
        }

        Dictionary<AbstractNode, HLNode> open = new Dictionary<AbstractNode, HLNode>();
        HashSet<AbstractNode> closed = new HashSet<AbstractNode>();

        HLNode start = new HLNode(startNode, null, 0f, HLHeuristic(startNode, goalNode));
        open[startNode] = start;

        while (open.Count > 0)
        {
            HLNode current = GetLowestFHL(open);

            if (current.abstractNode == goalNode)
            {
                return ReconstructHLPath(current);
            }

            open.Remove(current.abstractNode);
            closed.Add(current.abstractNode);

            foreach (AbstractEdge edge in current.abstractNode.edges)
            {
                AbstractNode neighbour = edge.to;

                if (closed.Contains(neighbour))
                {
                    continue;
                }

                float tentativeG = current.g + edge.cost;

                if (open.TryGetValue(neighbour, out HLNode existing))
                {
                    if (tentativeG < existing.g)
                    {
                        existing.g = tentativeG;
                        existing.parent = current;
                    }
                }
                else
                {
                    float h = HLHeuristic(neighbour, goalNode);
                    open[neighbour] = new HLNode(neighbour, current, tentativeG, h);
                }
            }
        }

        return null;
    }

    public static AbstractNode GetNearestNode(Vector3 worldPos,Vector2 chunkPos,List<AbstractNode> graph)
    {
        AbstractNode best = null;
        float bestDist = float.MaxValue;

        foreach (AbstractNode node in graph)
        {
            if (node.chunkPos != chunkPos)
            {
                continue;
            }

            float dist = Mathf.Abs(node.worldPos.x - worldPos.x) + Mathf.Abs(node.worldPos.y - worldPos.y);

            if (dist < bestDist)
            {
                bestDist = dist;
                best = node;
            }
        }

        return best;
    }
}
