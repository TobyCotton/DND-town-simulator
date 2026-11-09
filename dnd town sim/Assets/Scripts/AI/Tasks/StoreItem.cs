using System.Buffers.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoreItem : Task
{
    private Vector3 m_destination;
    StoreRoomScript m_storeRoom;
    Item m_itemToStore;

    public StoreItem(Item item, BaseAI ai)
    {
        m_itemToStore = item;
        m_baseAi = ai;
        m_PreRequisite.Add("InRange");
        m_effect = "ItemStored";
        m_priority = 3.0f;
        m_Weight = 1;
        m_Task = "StoreItem";
    }
    public StoreItem()
    {
        m_PreRequisite.Add("InRange");
        m_effect = "ItemStored";
        m_priority = 2.0f;
        m_Weight = 1;
        m_Task = "StoreItem";
        m_destination = Vector3.zero;
    }
    public override Vector3 getDestination()
    {
        StoreRoomScript[] storeRooms = Object.FindObjectsByType<StoreRoomScript>();
        float shortestDistance = Mathf.Infinity;
        Vector3 currentPosition = m_baseAi.transform.position;
        for (int i = 0; i < storeRooms.Length; i++)
        {
            float distance = Vector3.Distance(currentPosition, storeRooms[i].transform.position);
            if (distance < shortestDistance)
            {
                m_storeRoom = storeRooms[i];
                shortestDistance = distance;
                m_destination = storeRooms[i].transform.position;
            }
        }
        return m_destination;
    }

    public override bool Executing()
    {
        if (m_storeRoom)
        {
            m_storeRoom.m_Stored.Add(m_itemToStore);
            if (m_itemToStore.m_name == "Ore")
            {
                m_storeRoom.m_OreStored++;
            }
            else if (m_itemToStore.m_name == "Wood")
            {
                m_storeRoom.m_WoodStored++;
            }
            else
            {
                m_storeRoom.m_WheatStored++;
            }
            m_baseAi.m_Items.Remove(m_itemToStore);
        }
        else
        {
            TaskFailed();
            return false;
        }
        return true;
    }
}
