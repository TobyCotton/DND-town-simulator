using System.Buffers.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BuyWood : Task
{
    private NavMeshAgent m_agent;
    private Vector3 m_destination;
    private MarketScript m_market;
    public BuyWood(BaseAI ai)
    {
        m_Weight = 500;
        m_Task = "BuyWood";
        m_PreRequisite.Add("InRange");
        m_effect = "Wood";
        m_destination = Vector3.zero;
        m_baseAi = ai;
    }
    public BuyWood()
    {
        m_Weight = 500;
        m_Task = "BuyWood";
        m_PreRequisite.Add("InRange");
        m_effect = "Wood";
        m_destination = Vector3.zero;
    }
    public override Vector3 getDestination()
    {
        MarketScript[] markets = Object.FindObjectsByType<MarketScript>();
        float shortestDistance = Mathf.Infinity;
        Vector3 currentPosition = m_baseAi.transform.position;
        m_destination = Vector3.zero;
        for (int i = 0; i < markets.Length; i++)
        {
            float distance = Vector3.Distance(currentPosition, markets[i].transform.position);
            if (distance < shortestDistance)
            {
                m_market = markets[i];
                shortestDistance = distance;
                m_destination = markets[i].transform.position;
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
        m_market.m_money -= 2;
        m_baseAi.m_Items.Add(new Item("Wood", 1));
        return true;
    }
}
