using System.Buffers.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateHoe : Task
{
    private Vector3 m_destination;
    private BlacksmithScript m_BlacksmithScript;
    public CreateHoe(BaseAI ai)
    {
        m_Weight = 1;
        m_Task = "CreateHoe";
        m_PreRequisite.Add("InRange");
        m_PreRequisite.Add("Ore");
        m_PreRequisite.Add("Handle");
        m_effect = "Hoe";
        m_priority = 2.0f;
        m_destination = Vector3.zero;
        m_baseAi = ai;
    }
    public CreateHoe()
    {
        m_Weight = 1;
        m_Task = "CreateHoe";
        m_PreRequisite.Add("InRange");
        m_PreRequisite.Add("Ore");
        m_PreRequisite.Add("Handle");
        m_effect = "Hoe";
        m_priority = 2.0f;
        m_destination = Vector3.zero;
    }
    public override Vector3 getDestination()
    {
        if (m_baseAi.m_work)
        {
            if (!m_BlacksmithScript)
            {
                m_BlacksmithScript = m_baseAi.m_work.GetComponent<BlacksmithScript>();
            }
            m_destination = m_baseAi.m_work.transform.position;
            return m_destination;
        }
        return Vector3.zero;
    }
    public override bool Executing()
    {
        m_BlacksmithScript.m_Hoes++;
        m_BlacksmithScript.m_hoeRequested = false;
        Item toRemove = null;
        Item toRemove2 = null;
        foreach (Item item in m_baseAi.m_Items)
        {
            if (toRemove == null && item.m_name == "Ore")
            {
                toRemove = item;
            }
        }
        m_baseAi.m_Items.Remove(toRemove);
        foreach (Item item in m_baseAi.m_Items)
        {
            if (toRemove2 == null && item.m_name == "Handle")
            {
                toRemove2 = item;
            }
        }
        m_baseAi.m_Items.Remove(toRemove2);
        return true;
    }
}
