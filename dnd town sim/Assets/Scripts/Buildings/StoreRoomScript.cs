using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class StoreRoomScript : BaseBuilding
{
    public List<Item> m_Stored;
    public int m_WheatStored;
    public int m_WoodStored;
    public int m_OreStored;
    MerchantAI m_merchant;
    public bool m_requested = false;

    public StoreRoomScript()
    {
        m_Stored = new List<Item>();
    }

    void Update()
    {
        if (!m_merchant)
        {
            FindMerchant();
        }
        if (m_merchant && !m_requested)
        {
            if (m_WheatStored > 7)
            {
                m_requested = true;
                if (m_merchant.PriorityChecker(new Sell()))
                {
                    m_merchant.AddToTaskList(new Sell("Wheat", m_merchant, this), 0);
                    m_merchant.SetTaskList();
                }
                else
                {
                    m_merchant.m_toDoGoals.Add(new Sell("Wheat", m_merchant, this));
                }
            }
            if (m_WoodStored > 4)
            {
                m_requested = true;
                if (m_merchant.PriorityChecker(new Sell()))
                {
                    m_merchant.AddToTaskList(new Sell("Wood", m_merchant, this), 0);
                    m_merchant.SetTaskList();
                }
                else
                {
                    m_merchant.m_toDoGoals.Add(new Sell("Wood", m_merchant, this));
                }
            }
            if (m_OreStored > 4)
            {
                m_requested = true;
                if (m_merchant.PriorityChecker(new Sell()))
                {
                    m_merchant.AddToTaskList(new Sell("Ore", m_merchant, this), 0);
                    m_merchant.SetTaskList();
                }
                else
                {
                    m_merchant.m_toDoGoals.Add(new Sell("Ore", m_merchant, this));
                }
            }
        }
    }

    private void FindMerchant()
    {
        MerchantAI[] merchants = Object.FindObjectsByType<MerchantAI>();

        for (int i = 0; i < merchants.Length; i++)
        {
            m_merchant = merchants[i];
            return;
        }
    }
}
