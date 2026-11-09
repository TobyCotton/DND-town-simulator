using System.Buffers.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;

public class SmithyAI : BaseAI
{
    public SmithyAI()
    {
        m_goals.Add(new CreateHoe());
        m_goals.Add(new CreateHorseshoe());
        m_goals.Add(new CreateChisel());
        m_goals.Add(new CreateAxe());
        m_goals.Add(new CreatePickaxe());
        m_availableActions.Add(new GetOre());
        m_availableActions.Add(new GetWood());
        m_availableActions.Add(new BuyOre());
        m_availableActions.Add(new BuyWood());
        m_availableActions.Add(new GetHandle());
    }
    // Start is called before the first frame update
    void Start()
    {
        FindHome();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateToDo();
        foreach (Item item in m_Items)
        {
            Debug.Log(item.m_name);
        }
        if (m_tasks.Count == 1)
        {
            if (!m_tasks[0].m_executionStarted)
            {
                Task tempStore = m_tasks[0];
                m_tasks.Clear();
                AddToTaskList(tempStore, 0);
                SetTaskList();
            }
        }
    }

    public void AddToTaskList(Task goal, int listIncrement)
    {
        List<Task[]> saveBank = new List<Task[]>();
        bool triggerSave = false;
        if (!CheckPrerequisite(goal, listIncrement))
        {
            return;
        }
        for (int i = 0; i < goal.m_PreRequisite.Count; i++)
        {
            if (CheckInventory(goal, i))
            {
                return;
            }
            AddToList(goal, listIncrement, i);
            int temp = m_taskListOptions.Count;
            int found = listIncrement;
            Task[] Temptasks = m_taskListOptions[found].ToArray();
            for (int j = 0; j < m_availableActions.Count; j++)
            {
                if (goal.m_PreRequisite[i] == m_availableActions[j].m_effect)
                {
                    if (found >= listIncrement + 1)
                    {
                        int incrementer = 0;
                        foreach (Task[] list in saveBank)
                        {
                            m_taskListOptions.Add(FillTaskCopy(list));
                            incrementer++;
                        }
                        if (incrementer == 0)
                        {
                            m_taskListOptions.Add(FillTaskCopy(Temptasks));
                            incrementer++;
                        }
                        found = m_taskListOptions.Count - incrementer;
                        triggerSave = true;
                    }
                    switch (m_availableActions[j].m_Task)
                    {
                        case "Walk":
                            Vector3 destination = goal.getDestination();
                            if (destination != Vector3.zero)
                            {
                                AddToTaskList(new Walk(destination, this), found);
                            }
                            else
                            {
                                AddToTaskList(new Walk(destination, this), found);
                                Debug.Log("Walk failed in blacksmith");
                            }
                            break;
                        case "GetOre":
                            for (int k = 0; k < temp; k++)
                            {
                                AddToTaskList(new GetOre(this), found + k);
                            }
                            break;
                        case "GetWood":
                            for (int k = 0; k < temp; k++)
                            {
                                AddToTaskList(new GetWood(this), found + k);
                            }
                            break;
                        case "BuyOre":
                            for (int k = 0; k < temp; k++)
                            {
                                AddToTaskList(new BuyOre(this), found + k);
                            }
                            break;
                        case "BuyWood":
                            for (int k = 0; k < temp; k++)
                            {
                                AddToTaskList(new BuyWood(this), found + k);
                            }
                            break;
                        case "GetHandle":
                            for (int k = 0; k < temp; k++)
                            {
                                AddToTaskList(new GetHandle(this), found + k);
                            }
                            break;

                    }
                    found++;
                }
            }
            if (triggerSave)
            {
                triggerSave = false;
                foreach (List<Task> list in m_taskListOptions)
                {
                    saveBank.Add(list.ToArray());
                }
            }
        }
    }
}
