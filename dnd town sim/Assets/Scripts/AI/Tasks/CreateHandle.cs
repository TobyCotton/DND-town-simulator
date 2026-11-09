using System.Buffers.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateHandle : Task
{
    private Vector3 m_destination;
    private WoodWorkerScript m_WoodWorkerScript;
    public CreateHandle(BaseAI ai)
    {
        m_Weight = 1;
        m_Task = "CreateHandle";
        m_PreRequisite.Add("InRange");
        m_PreRequisite.Add("Wood");
        m_effect = "Handle";
        m_priority = 2.0f;
        m_destination = Vector3.zero;
        m_baseAi = ai;
    }
    public CreateHandle()
    {
        m_Weight = 1;
        m_Task = "CreateHandle";
        m_PreRequisite.Add("InRange");
        m_PreRequisite.Add("Wood");
        m_effect = "Handle";
        m_priority = 2.0f;
        m_destination = Vector3.zero;
    }
    public override Vector3 getDestination()
    {
        if (m_baseAi.m_work)
        {
            if (!m_WoodWorkerScript)
            {
                m_WoodWorkerScript = m_baseAi.m_work.GetComponent<WoodWorkerScript>();
            }
            m_destination = m_baseAi.m_work.transform.position;
            return m_destination;
        }
        return Vector3.zero;
    }
    public override bool Executing()
    {
        m_WoodWorkerScript.m_Handles++;
        m_WoodWorkerScript.m_handlesRequested = false;
        Item toRemove = null;
        foreach (Item item in m_baseAi.m_Items)
        {
            if (toRemove == null && item.m_name == "Wood")
            {
                toRemove = item;
            }
        }
        m_baseAi.m_Items.Remove(toRemove);
        return true;
    }
}
