using System.Buffers.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class HarvestWheat : Task
{
    private Vector3 m_destination;
    public HarvestWheat(BaseAI ai)
    {
        m_Weight = 1;
        m_Task = "HarvestWheat";
        m_PreRequisite.Add("InRange");
        m_PreRequisite.Add("Hoe");
        m_effect = "WheatHarvested";
        m_priority = 2.0f;
        m_destination = Vector3.zero;
        m_baseAi = ai;
    }
    public HarvestWheat()
    {
        m_Weight = 1;
        m_Task = "HarvestWheat";
        m_PreRequisite.Add("InRange");
        m_PreRequisite.Add("Hoe");
        m_effect = "WheatHarvested";
        m_priority = 2.0f;
        m_destination = Vector3.zero;
    }
    public override Vector3 getDestination()
    {
        if (m_baseAi.m_work)
        {
            m_destination = m_baseAi.m_work.transform.position;
            return m_destination;
        }
        return Vector3.zero;
    }
    public override bool Executing()
    {
        Item toRemove = null;
        for (int i = 0; i < m_baseAi.m_Items.Count; i++)
        {
            if (m_baseAi.m_Items[i].m_name == "Hoe")
            {
                if (!m_baseAi.m_Items[i].ReduceDurability())
                {
                    toRemove = m_baseAi.m_Items[i];
                }
            }
        }
        if (toRemove != null)
        {
            m_baseAi.m_Items.Remove(toRemove);
        }
        m_baseAi.m_Items.Add(new Item("Wheat", 1));
        m_baseAi.m_Items.Add(new Item("Wheat", 1));
        FarmScript farm = m_baseAi.m_work.GetComponent<FarmScript>();
        if (farm)
        {
            farm.m_requested = false;
        }
        m_baseAi.m_toDoGoals.Add(new StoreItem(m_baseAi.m_Items[m_baseAi.m_Items.Count - 1], m_baseAi));
        m_baseAi.m_toDoGoals.Add(new StoreItem(m_baseAi.m_Items[m_baseAi.m_Items.Count - 2], m_baseAi));
        return true;
    }
}