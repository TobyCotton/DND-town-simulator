using System.Buffers.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.FilePathAttribute;
using static UnityEditor.Progress;

public class Sell : Task
{
    StoreRoomScript m_storeRoom;
    string m_ItemName;
    public Sell(string item, BaseAI ai, StoreRoomScript storeRoom)
    {
        m_storeRoom = storeRoom;
        m_ItemName = item;
        m_Weight = 1;
        m_Task = "Sell";
        m_PreRequisite.Add("InRange");
        m_effect = "Sold";
        m_priority = 2.0f;
        m_baseAi = ai;
    }
    public Sell()
    {
        m_Weight = 1;
        m_Task = "Sell";
        m_PreRequisite.Add("InRange");
        m_effect = "Sold";
        m_priority = 2.0f;
        m_baseAi = null;
    }
    public override Vector3 getDestination()
    {
        return m_storeRoom.transform.position;
    }
    public override bool Executing()
    {
        Item toRemove = null;
        for (int i = 0; i < m_storeRoom.m_Stored.Count; i++)
        {
            if (m_storeRoom.m_Stored[i].m_name == m_ItemName)
            {
                toRemove = m_storeRoom.m_Stored[i];
            }
        }
        if (toRemove != null)
        {
            m_storeRoom.m_Stored.Remove(toRemove);
            MarketScript addMoney = m_baseAi.m_work.GetComponent<MarketScript>();
            if (addMoney != null)
            {
                addMoney.m_money++;
            }

        }
        m_storeRoom.m_requested = false;
        return true;
    }
}
