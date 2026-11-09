using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class WoodWorkerScript : BaseBuilding
{
    public int m_Handles;
    WittlerAI m_Wittler = null;
    public bool m_handlesRequested = false;

    public WoodWorkerScript()
    {
        m_Handles = 4;
    }
    // Update is called once per frame
    void Update()
    {
        if (!m_Wittler)
        {
            FindWittler();
        }
        if (m_Wittler)
        {
            if (m_Handles < 4 && !m_handlesRequested)
            {
                if (m_Wittler.PriorityChecker(new CreateHandle()))
                {
                    m_Wittler.AddToTaskList(new CreateHandle(m_Wittler), 0);
                    m_Wittler.SetTaskList();
                }
                else
                {
                    m_Wittler.m_toDoGoals.Add(new CreateHandle(m_Wittler));
                }
                m_handlesRequested = true;
            }
        }
    }

    private void FindWittler()
    {
        WittlerAI[] wittlers = Object.FindObjectsByType<WittlerAI>();

        for (int i = 0; i < wittlers.Length; i++)
        {
            if (!wittlers[i].m_work)
            {
                wittlers[i].m_work = this;
                m_Wittler = wittlers[i];
                return;
            }
        }
    }
}
