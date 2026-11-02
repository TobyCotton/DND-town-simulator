using UnityEngine;

public class Walk : Task
{
    private Vector3 m_destination;

    public Walk(Vector3 destination, BaseAI ai)
    {
        m_Weight = 1;
        m_Task = "Walk";
        m_destination = destination;
        m_effect = "InRange";
        m_time = 0;
        m_baseAi = ai;
    }

    // Template instance used by BaseAI's m_availableActions
    public Walk()
    {
        m_Weight = 1;
        m_Task = "Walk";
        m_destination = Vector3.zero;
        m_effect = "InRange";
    }

    public override void StartExecution()
    {
        m_executionStarted = true;
        m_failed = false;

        if (!m_baseAi.m_visible)
        {
            m_baseAi.ToggleMesh();
        }

        if (!m_baseAi.SetDestination(m_destination))
        {
            m_failed = true;
        }
    }

    public override bool Executing()
    {
        // No path or unwalkable target: end the task so the AI doesn't hang
        if (m_failed)
        {
            return true;
        }

        return !m_baseAi.IsMoving();
    }
}