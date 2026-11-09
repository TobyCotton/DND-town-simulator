using System.Buffers.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GetOre : Task
{
    private Vector3 m_destination;
    private StoreRoomScript m_storeRoom;
    public GetOre(BaseAI ai)
    {
        m_Weight = 0;
        m_Task = "GetOre";
        m_PreRequisite.Add("InRange");
        m_effect = "Ore";
        m_destination = Vector3.zero;
        m_baseAi = ai;
    }
    public GetOre()
    {
        m_Weight = 0;
        m_Task = "GetOre";
        m_PreRequisite.Add("InRange");
        m_effect = "Ore";
        m_destination = Vector3.zero;
    }
    public override Vector3 getDestination()
    {
        StoreRoomScript[] storeRooms = Object.FindObjectsByType<StoreRoomScript>();
        float shortestDistance = Mathf.Infinity;
        Vector3 currentPosition = m_baseAi.transform.position;
        m_destination = Vector3.zero;
        bool hasItem = false;
        for (int i = 0; i < storeRooms.Length; i++)
        {
            hasItem = false;
            foreach (Item items in storeRooms[i].m_Stored)
            {
                if (items.m_name == "Ore")
                {
                    hasItem = true;
                }
            }
            float distance = Vector3.Distance(currentPosition, storeRooms[i].transform.position);
            if (distance < shortestDistance && hasItem)
            {
                m_storeRoom = storeRooms[i];
                shortestDistance = distance;
                m_destination = storeRooms[i].transform.position;
            }
        }
        if (m_destination == Vector3.zero)
        {
            TaskFailed();
        }
        return m_destination;
    }
    public override bool Executing()
    {
        foreach (Item item in m_storeRoom.m_Stored)
        {
            if (item.m_name == "Ore")
            {
                m_baseAi.m_Items.Add(item);
                m_storeRoom.m_Stored.Remove(item);
                m_storeRoom.m_OreStored--;
                return true;
            }
        }
        TaskFailed();
        return false;
    }
}
