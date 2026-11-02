using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;
using static UnityEditor.Progress;
using static UnityEngine.GraphicsBuffer;

public class BaseAI : MonoBehaviour
{
    //Movement variables
    [SerializeField] float m_speed;
    [SerializeField] GameObject m_highlight;
    private GridManager m_gridManager;

    private List<Vector3> m_path = new List<Vector3>();
    private int m_pathIndex = 0;
    private bool m_currentlyMoving = false;

    //Functionality variables
    protected List<Task> m_availableActions = new List<Task>();
    protected List<Task> m_goals = new List<Task>();
    [System.NonSerialized]
    public List<Task> m_toDoGoals = new List<Task>();
    public NavMeshAgent m_agent;
    public BaseBuilding m_work;
    protected List<Task> m_tasks = new List<Task>();
    protected List<List<Task>> m_taskListOptions = new List<List<Task>>();
    [System.NonSerialized]
    public List<Item> m_Items = new List<Item>();
    public Vector3 m_homePosition = Vector3.zero;
    public float m_hunger;
    private bool m_skip = false;
    public bool m_visible = true;
    public bool m_multipleIdle = false;

    public BaseAI()
    {
        m_availableActions.Add(new Walk());
        m_goals.Add(new Idle());
        //m_goals.Add(new StoreItem());
        //m_work = null;
        //m_hunger = 0;
        //m_goals.Add(new GetFood());
    }

    private void Start()
    {
        m_currentlyMoving = false;
    }
    public bool PriorityChecker(Task a)//check if priority is greater than current task
    {
        if (a.m_cooldownPeriod > 0)
        {
            return false;
        }
        if (m_tasks.Count == 0)
        {
            return true;
        }
        if (a.m_priority > m_tasks[0].m_priority)
        {
            if (m_tasks[0] is Idle) { m_multipleIdle = false; }
            else
            {
                m_tasks[0].m_executionStarted = false;
                m_toDoGoals.Add(m_tasks[0]);
            }
            m_tasks.Clear();
            return true;
        }
        return false;
    }
    public void SetTaskList()//Choose the cheapest option to do primary task
    {
        float lowestWeight = Mathf.Infinity;
        List<Task> tempTasks = new List<Task>();
        bool failedTask = false;
        for (int i = 0; i < m_taskListOptions.Count; i++)
        {
            failedTask = false;
            float currentWeight = 0;
            for (int j = 0; j < m_taskListOptions[i].Count; j++)
            {
                if (m_taskListOptions[i][j].m_failed)
                {
                    failedTask = true;
                }
                currentWeight += m_taskListOptions[i][j].m_Weight;
            }
            if (currentWeight < lowestWeight && !failedTask)
            {
                lowestWeight = currentWeight;
                tempTasks = m_taskListOptions[i];
            }
        }
        if (tempTasks.Count == 0)
        {
            m_taskListOptions[0][0].TaskFailed();
            m_toDoGoals.Add(m_taskListOptions[0][0]);
            m_taskListOptions.Clear();
            return;
        }
        m_tasks.Clear();
        for (int i = 0; i < tempTasks.Count; i++)
        {
            m_tasks.Add(tempTasks[i]);
        }
        m_tasks[m_tasks.Count - 1].StartExecution();
        m_multipleIdle = false;
        m_taskListOptions.Clear();
    }
    protected List<Task> FillTaskCopy(Task[] toCopy)
    {
        List<Task> temp = new List<Task>();
        for (int i = 0; i < toCopy.Length; i++)
        {
            temp.Add(toCopy[i]);
        }
        return temp;
    }
    protected bool CheckPrerequisite(Task goal, int listIncrement)
    {
        if (goal.m_PreRequisite.Count == 0)
        {
            if (m_taskListOptions.Count == 0)
            {
                m_taskListOptions.Add(new List<Task>());
            }
            m_taskListOptions[listIncrement].Add(goal);
            return false;
        }
        return true;
    }
    protected bool CheckInventory(Task goal, int prerequisiteNumber)
    {
        for (int z = 0; z < m_Items.Count; z++)
        {
            if (goal.m_PreRequisite[prerequisiteNumber] == m_Items[z].m_name)
            {
                return true;
            }
        }
        return false;
    }
    protected void AddToList(Task goal, int listIncrement, int prerequisiteNumber)
    {
        if (prerequisiteNumber == 0)//There is a prerequisite we have to do so add the goal only do on the first time through
        {
            if (m_taskListOptions.Count == listIncrement)
            {
                m_taskListOptions.Add(new List<Task>());
            }
            m_taskListOptions[listIncrement].Add(goal);
        }
    }
    public void CantComplete()
    {
        m_tasks[0].m_executionStarted = false;
        m_tasks[0].m_priority--;
        m_toDoGoals.Add(m_tasks[0]);
        m_tasks.Clear();
    }
    protected void UpdateToDo()
    {
        foreach (Task task in m_toDoGoals)
        {
            task.UpdatecoolDownPeriod();
        }
        m_hunger += Time.deltaTime / 2;
        //if (m_hunger > 60.0f)//if hungry go eat
        //{
        //    if (m_tasks.Count != 0)
        //    {
        //        if (m_tasks[0] is GetFood) { }
        //        else
        //        {
        //            bool isIn = false;
        //            for (int i = 0; i < m_toDoGoals.Count; i++)
        //            {
        //                if (m_toDoGoals[i] is GetFood)
        //                {
        //                    isIn = true;
        //                }
        //            }
        //            if (!isIn)// we are hungry and no current goal for it present
        //            {
        //                if (PriorityChecker(new GetFood()))
        //                {
        //                    m_tasks.Add(new GetFood(m_agent, this));
        //                    m_skip = true;
        //                }
        //                else
        //                {
        //                    m_toDoGoals.Add(new GetFood(m_agent, this));
        //                }
        //            }
        //        }
        //    }
        //    else
        //    {
        //        m_tasks.Add(new GetFood(m_agent, this));
        //        m_skip = true;
        //    }
        //}
        int taskLength = m_tasks.Count;
        if (taskLength != 0)//There is a task to execute
        {
            Task stored = null;
            for (int i = 0; i < m_toDoGoals.Count; i++)//check if task priority is greater than current executing task
            {
                if (stored == null)
                {
                    if (PriorityChecker(m_toDoGoals[i]))
                    {
                        stored = m_toDoGoals[i];
                    }
                }
                else
                {
                    if (stored.m_priority < m_toDoGoals[i].m_priority)
                    {
                        stored = m_toDoGoals[i];
                    }
                }
            }
            if (stored != null)
            {
                m_toDoGoals.Remove(stored);
                m_tasks.Add(stored);
                m_skip = true;
            }
            if (!m_skip)//Don't skip task
            {
                m_tasks[m_tasks.Count - 1].m_time -= Time.deltaTime;
                if (m_tasks[m_tasks.Count - 1].m_time <= 0.0f && m_tasks[taskLength - 1].Executing())
                {
                    if (m_tasks[taskLength - 1] is Idle)
                    {
                        if (m_visible)
                        {
                            ToggleMesh();
                        }
                        m_multipleIdle = true;
                    }
                    m_tasks.RemoveAt(taskLength - 1);
                    taskLength = m_tasks.Count;
                    if (taskLength != 0)
                    {
                        m_tasks[taskLength - 1].StartExecution();
                    }
                }
                else
                {
                    if (m_tasks[taskLength - 1].m_failed)
                    {
                        m_tasks[0].TaskFailed();
                        m_tasks[0].m_executionStarted = false;
                        m_toDoGoals.Add(m_tasks[0]);
                        m_tasks.Clear();
                    }
                }
            }
            m_skip = false;
        }
        else
        {
            if (m_toDoGoals.Count != 0)//Do we have tasks to complete
            {
                Task stored = null;
                for (int i = 0; i < m_toDoGoals.Count; i++)
                {
                    if (stored != null)
                    {
                        if (stored.m_priority < m_toDoGoals[i].m_priority)
                        {
                            stored = m_toDoGoals[i];
                        }
                    }
                    else
                    {
                        if (m_toDoGoals[i].m_cooldownPeriod <= 0)
                        {
                            stored = m_toDoGoals[i];
                        }
                    }
                }
                if (stored != null)
                {
                    m_tasks.Add(stored);
                    m_toDoGoals.Remove(stored);
                    return;
                }
            }
            if (m_homePosition != Vector3.zero)//go home if no tasks to complete
            {
                m_tasks.Add(new Idle(m_homePosition, this));
                m_tasks[0].StartExecution();
            }
        }
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
    private void Update()
    {
        UpdateToDo();
    }
    public void ToggleMesh()
    {
        if (m_visible)
        {
            m_visible = false;
            GetComponent<SpriteRenderer>().enabled = false;
        }
        else
        {
            m_visible = true;
            GetComponent<SpriteRenderer>().enabled = true;
        }
    }
    public void SetGridManager(GridManager gridManager)
    {
        m_gridManager = gridManager;
    }
    public bool SetDestination(Vector3 destination)
    {
        if (!m_gridManager.IsWalkable(destination))
        {
            Debug.LogWarning($"{name}: destination {destination} is not walkable, ignoring.");
            return false;
        }

        m_gridManager.WorldToGrid(transform.position, out Vector2 startChunk, out Vector2 startTile);
        m_gridManager.WorldToGrid(destination, out Vector2 goalChunk, out Vector2 goalTile);

        // Already on the goal tile, so there is nothing to walk
        if (startChunk == goalChunk && startTile == goalTile)
        {
            m_path.Clear();
            m_pathIndex = 0;
            m_currentlyMoving = false;
            return true;
        }

        List<Vector3> path = BuildFullPath(transform.position, destination);

        if (path == null || path.Count == 0)
        {
            Debug.LogWarning($"{name}: No path found to {destination}");
            return false;
        }

        m_path = path;
        m_pathIndex = 0;
        m_currentlyMoving = true;
        m_highlight.SetActive(false);
        return true;
    }

    public bool IsMoving()
    {
        return m_currentlyMoving;
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

        if (startChunk == goalChunk)
        {
            return LowLevelToWorld(startChunk, Pathfinder.FindPathInChunk(
                startChunk, startTile, goalTile, m_gridManager, m_gridManager.m_chunksSide));
        }

        // Build temporary nodes for start and goal
        AbstractNode startNode = Pathfinder.BuildTemporaryNode(
            startWorld, startChunk, startTile, graph, m_gridManager);
        AbstractNode goalNode = Pathfinder.BuildTemporaryNode(
            goalWorld, goalChunk, goalTile, graph, m_gridManager);

        if (startNode == null || goalNode == null)
        {
            Debug.LogWarning($"{name}: could not connect start or goal to abstract graph");

            foreach (AbstractNode node in graph)
            {
                node.edges.RemoveAll(e => e.to == startNode || e.to == goalNode);
            }
            return null;
        }

        List<AbstractNode> hlPath = Pathfinder.FindHighLevelPath(startNode, goalNode);

        if (hlPath == null || hlPath.Count == 0)
        {
            foreach (AbstractNode node in graph)
            {
                node.edges.RemoveAll(e => e.to == startNode || e.to == goalNode);
            }

            return null;
        }

        List<Vector3> fullPath = new List<Vector3>();
        Vector2 currentChunk = startChunk;
        Vector2 currentTile = startTile;

        foreach (AbstractNode waypoint in hlPath)
        {
            if (waypoint.chunkPos == currentChunk)
            {
                AppendLowLevel(fullPath, currentChunk, currentTile, waypoint.tilePos);
            }
            else
            {
                AbstractNode mirror = GetMirrorNode(waypoint, currentChunk);
                if (mirror != null)
                {
                    AppendLowLevel(fullPath, currentChunk, currentTile, mirror.tilePos);
                }
                fullPath.Add(new Vector3(waypoint.worldPos.x, waypoint.worldPos.y, -1f));
            }

            currentChunk = waypoint.chunkPos;
            currentTile = waypoint.tilePos;
        }

        AppendLowLevel(fullPath, currentChunk, currentTile, goalTile);

        foreach (AbstractNode node in graph)
        {
            node.edges.RemoveAll(e => e.to == startNode || e.to == goalNode);
        }

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
    private AbstractNode GetMirrorNode(AbstractNode crossingNode, Vector2 currentChunk)
    {
        // The mirror is the node in currentChunk that has an edge pointing to crossingNode
        List<AbstractNode> graph = m_gridManager.GetAbstractGraph();

        foreach (AbstractNode node in graph)
        {
            if (node.chunkPos != currentChunk)
            {
                continue;
            }

            foreach (AbstractEdge edge in node.edges)
            {
                if (edge.to == crossingNode)
                {
                    return node;
                }
            }
        }

        Debug.LogWarning($"No mirror node found in chunk {currentChunk} for crossing node {crossingNode.worldPos}");
        return null;
    }
}
