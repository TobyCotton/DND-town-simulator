using UnityEngine;

public class Idle : Task
{
    private Vector3 m_destination;

    public Idle(Vector3 destination, BaseAI ai)
    {
        m_Weight = 1;
        m_Task = "Idle";
        m_destination = destination;
        m_effect = "InRange";
        m_time = 0;
        m_baseAi = ai;
    }

    // Template instance used by BaseAI's m_availableActions
    public Idle()
    {
        m_Weight = 1;
        m_Task = "Idle";
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
        return false;
    }
}