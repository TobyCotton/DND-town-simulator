using System.Collections.Generic;
using UnityEngine;

public class Task//Create a cooldown period for tasks if they fail
{
    public List<string> m_PreRequisite;
    public string m_Task;
    public int m_Weight;
    public string m_effect;
    public float m_priority = -1.0f;
    public bool m_executionStarted;
    public float m_time = 2.0f;
    protected BaseAI m_baseAi;
    public float m_cooldownPeriod = 0f;
    public bool m_failed = false;

    public Task()
    {
        m_PreRequisite = new List<string>();
        m_executionStarted = false;
    }

    public void UpdatecoolDownPeriod()
    {
        if (m_cooldownPeriod > 0)
        {
            m_cooldownPeriod -= Time.deltaTime;
            if (m_cooldownPeriod <= 0)
            {
                m_cooldownPeriod = 0;
                m_failed = false;
            }
        }
    }
    public virtual void StartExecution()
    {
        m_executionStarted = true;
        if (m_baseAi.m_visible)
        {
            m_baseAi.ToggleMesh();
        }
    }
    public virtual bool Executing()
    {
        return true;
    }
    public virtual Vector3 getDestination() { return Vector3.zero; }

    public void TaskFailed()
    {
        m_failed = true;
        m_cooldownPeriod = 15.0f;
    }
}
