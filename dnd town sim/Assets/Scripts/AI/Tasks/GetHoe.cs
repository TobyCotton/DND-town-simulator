using System.Buffers.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GetHoe : Task
{
    private Vector3 m_destination;
    private BlacksmithScript m_blacksmith;
    public GetHoe(BaseAI ai)
    {
        m_Weight = 0;
        m_Task = "GetHoe";
        m_PreRequisite.Add("InRange");
        m_effect = "Hoe";
        m_destination = Vector3.zero;
        m_baseAi = ai;
    }
    public GetHoe()
    {
        m_Weight = 0;
        m_Task = "GetHoe";
        m_PreRequisite.Add("InRange");
        m_effect = "Hoe";
        m_destination = Vector3.zero;
    }
    public override Vector3 getDestination()
    {
        BlacksmithScript[] blacksmiths = Object.FindObjectsByType<BlacksmithScript>();
        float shortestDistance = Mathf.Infinity;
        Vector3 currentPosition = m_baseAi.transform.position;
        for (int i = 0; i < blacksmiths.Length; i++)
        {
            float distance = Vector3.Distance(currentPosition, blacksmiths[i].transform.position);
            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                m_destination = blacksmiths[i].transform.position;
                m_blacksmith = blacksmiths[i];
            }
        }
        return m_destination;
    }
    public override bool Executing()
    {
        if (m_baseAi)
        {
            if (m_blacksmith.m_Hoes > 0)
            {
                m_baseAi.m_Items.Add(new Item("Hoe", 2));
                m_blacksmith.m_Hoes--;
            }
            else
            {
                TaskFailed();
                return false;
            }
        }
        return true;
    }
}